# MassTransitPhantomMessages

Test #1
1.  Start A
2.  Start B
3.  Kill or Ctrl-C or X or q1 or q2 B (q2 explicitly cancels the scedule)
4.  Observe in RabbitMQ web management that messages are still produced in the BMessage queue
5.  Stop A, A and B messages stop producing as seen in web management

Test #2
1.  Start B
2.  Start A
3.  Stop B
4.  B messages stop producing

I would expect that stopping the B process you stop scheduling B messages.
