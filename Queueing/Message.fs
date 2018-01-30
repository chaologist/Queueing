namespace Queueing
type QueueMessage<'TMessage>= {MessageId:System.Guid;Body:'TMessage}
type SerializedMessage = {MessageId:System.Guid;Bytes:byte[]}
type Enqueueable<'TMessage>=
    | Raw of 'TMessage
    | Message of QueueMessage<'TMessage>


