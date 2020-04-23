module Agent

/// A wrapper for MailboxProcessor that catches all unhandled exceptions
/// and reports them via the 'OnError' event. Otherwise, the API
/// is the same as the API of 'MailboxProcessor'
type Agent<'T>(f:Agent<'T> -> Async<unit>) as self =
  // Create an event for reporting errors
  let errorEvent = Event<_>()
  // Start standard MailboxProcessor
  let inbox = new MailboxProcessor<'T>(fun _ ->
    async {
      // Run the user-provided function & handle exceptions
      try return! f self
      with e -> errorEvent.Trigger(e)
    })

  /// Triggered when an unhandled exception occurs
  member __.OnError = errorEvent.Publish

  member __.Trigger exn = errorEvent.Trigger exn

  /// Starts the mailbox processor
  member __.Start() = inbox.Start()
  /// Receive a message from the mailbox processor
  member __.Receive() = inbox.Receive()
  /// Post a message to the mailbox processor
  member __.Post(value:'T) = inbox.Post value

  member __.PostAndReply(f: AsyncReplyChannel<'a> -> 'T) = inbox.PostAndReply f

  member __.PostAndAsyncReply(f: AsyncReplyChannel<'a> -> 'T) = inbox.PostAndAsyncReply f

  /// Start the mailbox processor
  static member Start f =
    let agent = new Agent<_>(f)
    agent.Start()
    agent

