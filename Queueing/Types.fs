namespace Queueing.Definitions
    open System
    type OutboundRouting<'TOut>(routing:string)=
        member public this.Routing = routing

    type QueueDefinition<'TIn> (name:string, inboundRoutings:seq<OutboundRouting<'TIn>>) =
        member public this.Name = name
        member public this.Routings = inboundRoutings

    type QueueClientDefinition<'TIn,'TOut> (inboundDefinition:QueueDefinition<'TIn>,outboundRoutings:seq<OutboundRouting<'TOut>>, payload:('TIn->'TOut)) = 
        member public this.InboundDefinition = inboundDefinition
        member public this.OutboundRoutings = outboundRoutings 
        member public this.Payload = payload

    type AckNack =
        | Ack
        | Nack of bool