namespace Queueing
open Newtonsoft.Json
open Queueing.Definitions
open Pipelines
open Pipelines.Pipeline
open Pipelines.Attempt

type QueueClientSteps=
    | Work =1


type QueueClientProcessor<'TIn,'TOut>(outputEnqueuer:IEnqueuer<'TOut>, acknacker:AckNack->unit, transform:'TIn->ExecutionDecision<'TOut>)=
    
    let telemetrize step f  =
        Telemetry.Sinks.Wrap [|Telemetry.ApplicationInsights.Sink.Sink; Telemetry.Log4Net.Sink.Sink|] step f
    
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
    let exceptionEvaluator = new ExceptionEvaluator.Evaluator()

    member this.Process(raw:byte[])=
        let shouldRequeue()=
            not ( exceptionEvaluator.Evaluate(raw))

        let flow() =
            let transformMessage msg=
                let res=msg.Body|> transform
                {MessageId = msg.MessageId;Body=res}
            let enqueue msg =
                match msg.Body with
                    | DidNotExecute -> Ack
                    | Executed (m)->        
                        outputEnqueuer.Enqueue(m,msg.MessageId)
                        Ack

            let p =pipeline {
                let! msg = raw >?> (telemetrize QueueClientSteps.Work unGlueMessageArrays)
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
                    acknacker(Nack(shouldRequeue()))
                    raise err
        flow ()
    

