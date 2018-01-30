namespace Queueing.RabbitMq.Definitions
    type MessageService (hostName:string) =
        member public this.HostName = hostName

    type Exchange (name:string) =
        member public this.Name = name

