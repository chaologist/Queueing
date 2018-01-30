namespace Queueing
type IQueueClientFactory<'TIn,'TOut,'TClient when 'TClient :> IQueueClient<'TIn,'TOut> > =
    abstract member GiveMeOne: unit->'TClient
