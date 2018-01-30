namespace Queueing
   
type IQueueClient<'TIn,'TOut>=
    abstract member Process:'TIn->'TOut

