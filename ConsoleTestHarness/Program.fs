open System
open Queueing.Definitions
open Queueing.RabbitMq
open RabbitMqModelFactory
open System.Xml
open System.Reflection
open System.IO

[<EntryPoint>]
let main argv =

    //trying to get log 4 net to work
    let log4netConfig = new XmlDocument();
    log4netConfig.Load(File.OpenRead("log4net.config"));
    let repo = log4net.LogManager.CreateRepository(Assembly.GetEntryAssembly(), typeof<log4net.Repository.Hierarchy.Hierarchy>);
    ignore(log4net.Config.XmlConfigurator.Configure(repo, log4netConfig.["log4net"]))


    let msgService = new Queueing.RabbitMq.Definitions.MessageService("localhost")
    let xchange = new Queueing.RabbitMq.Definitions.Exchange ("SomeExchange")
    let routing1 = new OutboundRouting<string> ("rout1")
    let routing2 = new OutboundRouting<string> ("rout2")
    let routing3 = new OutboundRouting<string> ("rout3")
    
    let fact = new RabbitMqModelFactory(RabbitMqConnectionPool.GetConnectionFactory(msgService.HostName))
    let eneueuer = new RabbitMqEnqueuer<string>((fun ()-> fact.Model),xchange,[|routing1;routing2;routing3|]) :> Queueing.IEnqueuer<string>

    eneueuer.Enqueue "this is my test"
    let r = new System.Random()

    let work2 (label:string) t=
//        System.Console.WriteLine(label)
        [|System.DateTime.Now.ToLongTimeString()|] |> Seq.ofArray
        

    let work (x)=
 //       System.Console.WriteLine(x.ToString())
        System.DateTime.Now.ToLongTimeString()

    let clientDef = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "one")
    let clientDef2 = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "two")
    let clientDef3 = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "tre")
    let clientDef4 = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "for")
    let clientDef5 = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "fiv")
    let clientDef6 = new QueueClientDefinition<string,string> (new QueueDefinition<string>("TestQueue",[|routing1;routing2|]),[|routing1;routing2;routing3|], work2 "six")

    use client = new RabbitMqClient<string,string> (msgService,xchange,clientDef)
    use client2 = new RabbitMqClient<string,string> (msgService,xchange,clientDef2)
    use client3 = new RabbitMqClient<string,string> (msgService,xchange,clientDef3)
    use client4 = new RabbitMqClient<string,string> (msgService,xchange,clientDef4)
    use client5 = new RabbitMqClient<string,string> (msgService,xchange,clientDef5)
    use client6 = new RabbitMqClient<string,string> (msgService,xchange,clientDef6)

    

    let d = System.Console.ReadLine()
    printfn "%A" argv
    0 // return an integer exit code
