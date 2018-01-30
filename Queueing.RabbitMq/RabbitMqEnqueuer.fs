namespace Queueing.RabbitMq
open RabbitMQ.Client
open Queueing
open System
type RabbitMqEnqueuer<'TMessage>(msgService:Definitions.MessageService,exchange:Definitions.Exchange,routings:seq<Definitions.OutboundRouting<'TMessage>>)  =
    inherit Queueing. BaseEnqueuer<'TMessage>()

    let factory = new ConnectionFactory()
    do
        factory.HostName <- msgService.HostName
    
    let connection = lazy(factory.CreateConnection())
    let channel = lazy(connection.Value.CreateModel())

    override this.Enqueue (bytes:byte[])=
        channel.Value.ExchangeDeclare (exchange.Name,"topic",true)
        channel.Value.BasicPublish (exchange.Name,String.concat "." (routings|>Seq.map (fun r->r.Routing)),null,bytes)
        ()
    override this.EnqueueAsync (msg)=
        async{
            ()
         }

    interface IDisposable with
        member this.Dispose() =
            if (channel.IsValueCreated) then
                channel.Value.Dispose()
            if (connection.IsValueCreated) then
                connection.Value.Dispose()
            