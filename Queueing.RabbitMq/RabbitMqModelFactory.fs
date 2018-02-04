module RabbitMqModelFactory
open System.Threading
open RabbitMQ.Client

type RabbitMqModelFactory (connectionFactory:unit->IConnection)=
     let model = new ThreadLocal<_> (fun ()-> connectionFactory().CreateModel())   
     member this.Model = model.Value