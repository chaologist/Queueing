open System
open Queueing.Definitions
open Queueing.RabbitMq
open RabbitMqModelFactory

[<EntryPoint>]
let main argv =
    let msgService = new Queueing.RabbitMq.Definitions.MessageService("localhost")
    let xchange = new Queueing.RabbitMq.Definitions.Exchange ("SomeExchange")
    let routing1 = new OutboundRouting<string> ("rout1")
    let routing2 = new OutboundRouting<string> ("rout2")
    let routing3 = new OutboundRouting<string> ("rout3")
    
    let fact = new RabbitMqModelFactory(RabbitMqConnectionPool.GetConnectionFactory(msgService.HostName))
    let eneueuer = new RabbitMqEnqueuer<string>((fun ()-> fact.Model),xchange,[|routing1;routing2;routing3|]) :> Queueing.IEnqueuer<string>

    eneueuer.Enqueue "this is my test"
    
    let work (x)=
        System.Console.WriteLine(x.ToString())
        System.DateTime.Now.ToLongTimeString()

    let clientDef = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work)

    use client = new RabbitMqClient<string,string> (msgService,xchange,clientDef)
    use client2 = new RabbitMqClient<string,string> (msgService,xchange,clientDef)

    let d = System.Console.ReadLine()
    printfn "%A" argv
    0 // return an integer exit code
