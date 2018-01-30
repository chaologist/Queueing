namespace Queueing
open System
open Newtonsoft.Json
open Pipelines
open Pipelines.Pipeline
open Pipelines.Attempt

[<AbstractClass>]
type BaseEnqueuer<'TMessage> () as this=
    let serialize thing=
        thing |> JsonConvert.SerializeObject
    let encode (string:string)=
        System.Text.Encoding.UTF8.GetBytes(string)
    let serializeAndEncode thing =
        thing |> serialize |> encode
    let createMessage msg=
        match msg with
            | Raw (raw) -> {MessageId=System.Guid.NewGuid();Body=raw}
            | Message (message) -> message
    let serializeMsg (msg:QueueMessage<'TMessage>)=
        {MessageId=msg.MessageId;Bytes=serializeAndEncode(msg.Body)}
    let glueMessageArrays (serMsg:SerializedMessage)=
        Array.append (serMsg.MessageId.ToByteArray()) serMsg.Bytes
    let enqueue (byteSink:byte[]->unit) thing=
        let pipe =pipeline{
            let! msg = thing >?> createMessage
            let! smsg = msg >?> serializeMsg
            let! bytes = smsg >?> glueMessageArrays
            let! res =bytes >?> byteSink
            return res
        }
        pipe()

    abstract member Enqueue: byte[]->unit
    abstract member EnqueueAsync: 'TMessage->Async<unit>

    interface IEnqueuer<'TMessage>  with
         member i.Enqueue(msg) = 
            let res =enqueue this.Enqueue (Raw(msg))
            match res with
                | Success(u) ->
                    ()
                | Failure (err)->
                    raise err

         member i.Enqueue(msg,messageId) = 
            let res =enqueue this.Enqueue (Message({MessageId = messageId;Body=msg}))
            match res with
                | Success(u) ->
                    ()
                | Failure (err)->
                    raise err

