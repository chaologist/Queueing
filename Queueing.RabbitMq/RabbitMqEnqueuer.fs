﻿namespace Queueing.RabbitMq
open RabbitMQ.Client
open Queueing
open System
open RabbitMqConnectionPool

type RabbitMqEnqueuer<'TMessage>(modelFactory:unit->IModel,exchange:Definitions.Exchange,routings:seq<Definitions.OutboundRouting<'TMessage>>)  =
    inherit Queueing. BaseEnqueuer<'TMessage>()

    let channel = lazy(modelFactory())
    let exch = lazy(channel.Value.ExchangeDeclare (exchange.Name,"topic",true))

    override this.Enqueue (bytes:byte[])=
        let un = exch.Value
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
            