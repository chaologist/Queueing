namespace Queueing.RabbitMq
open RabbitMQ.Client
open RabbitMQ.Client.Events
open Queueing
open Queueing.Definitions
open System


type RabbitMqClient<'TIn,'TOut> (msgService:Definitions.MessageService,exchange:Definitions.Exchange,def:QueueClientDefinition<'TIn,'TOut>) as this=
    let factory = new ConnectionFactory()
    let n = factory.HostName<-msgService.HostName
    let connection = factory.CreateConnection()
    let channel = connection.CreateModel()
    let dec= channel.ExchangeDeclare(exchange.Name, "topic",true);
    let queueName = channel.QueueDeclare(def.InboundDefinition.Name,true,false,false)        
    let m= def.InboundDefinition.Routings |> Seq.iter (fun ib->channel.QueueBind(def.InboundDefinition.Name,exchange.Name,"#."+ib.Routing+".#"))
    let consumer = new EventingBasicConsumer(channel)
    let enqueuer = new RabbitMqEnqueuer<'TOut>(msgService,exchange,def.OutboundRoutings) :>IEnqueuer<'TOut>

    let handle_receive (ea:BasicDeliverEventArgs) = 
        let acknacker an =
            if channel.IsOpen then
                match an with
                    | Ack ->
                            channel.BasicAck(ea.DeliveryTag,false)
                    | Nack(requeue)->
                        channel.BasicNack(ea.DeliveryTag,false,requeue)
                    
        let processor = Queueing.QueueClientProcessor(enqueuer, acknacker, def.Payload)
        processor.Process ea.Body
        ()    
    do
        consumer.Received.Add handle_receive
        let s = channel.BasicConsume(def.InboundDefinition.Name,false,consumer)
        ()
    interface IDisposable with
        member x.Dispose()=
            channel.Dispose()
            connection.Dispose()

