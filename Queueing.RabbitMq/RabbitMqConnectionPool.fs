module RabbitMqConnectionPool
open RabbitMQ.Client

let factorypool = new System.Collections.Concurrent.ConcurrentDictionary<string,IConnectionFactory>()
let pool = new System.Collections.Concurrent.ConcurrentDictionary<string,IConnection>()
let createConnectionFactory hn=
    let factory = new ConnectionFactory()
    factory.HostName <- hn
    factory.AutomaticRecoveryEnabled <-true
    factory :> IConnectionFactory

let GetConnection(hostName)=
    let factory = factorypool.GetOrAdd(hostName,createConnectionFactory)
    pool.GetOrAdd (hostName,fun hn->factory.CreateConnection())
let GetConnectionFactory(hostName)=
    let f () = 
        GetConnection(hostName)
    f

