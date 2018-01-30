namespace Queueing

type IEnqueuer<'TMessage> = 
    abstract member Enqueue : 'TMessage->unit
    abstract member Enqueue : 'TMessage*System.Guid->unit
