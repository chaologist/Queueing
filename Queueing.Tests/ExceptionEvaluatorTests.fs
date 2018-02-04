module ExceptionEvaluatorTests

open System
open Xunit

[<Fact>]
let FirstIsFalse () =
    let subject = new Queueing.ExceptionEvaluator.Evaluator();
    let message = System.Guid.NewGuid().ToByteArray()
    let result = subject.Evaluate(message);
    Assert.False(result)
    ()

[<Fact>]
let FourthIsFalse () =
    let subject = new Queueing.ExceptionEvaluator.Evaluator();
    let message = System.Guid.NewGuid().ToByteArray()

    let result = {1..4} |> Seq.map (fun x-> subject.Evaluate(message))
                        |> Seq.fold (fun a e-> a||e) false

    Assert.False(result)     
    ()
[<Fact>]
let FifthIsTrue () =
    let subject = new Queueing.ExceptionEvaluator.Evaluator();
    let message = System.Guid.NewGuid().ToByteArray()

    let result = {1..5} |> Seq.map (fun x-> subject.Evaluate(message))
                        |> Seq.fold (fun a e-> a||e) false

    Assert.True(result)     
    ()
