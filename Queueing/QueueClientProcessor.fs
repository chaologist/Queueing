namespace Queueing
open Newtonsoft.Json
open Queueing.Definitions
open Pipelines
open Pipelines.Pipeline
open Pipelines.Attempt
type QueueClientProcessor<'TIn,'TOut>(outputEnqueuer:IEnqueuer<'TOut>, acknacker:AckNack->unit, transform:'TIn->'TOut)=
    let unGlueMessageArrays (rawBytes:byte[])=
        let guid = new System.Guid(rawBytes |> Array.take 16)
        let rest = rawBytes |> Array.skip 16
        {MessageId = guid;Bytes=rest}
    let deserialize str=
        str |> JsonConvert.DeserializeObject<'TIn>
    let disencode (bytes:byte[])=
        System.Text.Encoding.UTF8.GetString(bytes)
    let disencodeAndDeserialize bytes =
        bytes |> disencode |> deserialize
    let deserializeMsg rawMsg=
        {MessageId=rawMsg.MessageId; Body=rawMsg.Bytes |>disencodeAndDeserialize}        

    member this.Process(raw:byte[])=
        let flow() =
            let transformMessage msg=
                let res=msg.Body|> transform
                {MessageId = msg.MessageId;Body=res}
            let enqueue msg =
                outputEnqueuer.Enqueue(msg.Body,msg.MessageId)
                Ack
            let p =pipeline {
                let! msg = raw >?> unGlueMessageArrays
                let! dser = msg >?> deserializeMsg
                let! processed = dser >?> transformMessage
                let! enqueued = processed >?> enqueue
                return enqueued
            }
            let result=p()
            match result with 
                | Success (acknack)->
                    acknacker(acknack)
                | Failure (err)->
                    acknacker(Nack(false))
                    raise err
        flow ()
    

