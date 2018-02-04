namespace Queueing
open Queueing.murmurHash
module ExceptionEvaluator =
    let dictionary = new System.Collections.Concurrent.ConcurrentDictionary<uint32,int>()    
    type public Evaluator()=
        member public this.Evaluate (msgBytes:byte[])=
            let msgHash = murmurhash3 msgBytes 0u
            let v = dictionary.AddOrUpdate(msgHash,1,fun k e->e+1)
            v > 4 //TODO: pull this threshold out into a setting
