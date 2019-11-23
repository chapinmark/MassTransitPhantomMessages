using MassTransit;
using MassTransit.Scheduling;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ScheduleAndConsumeB
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var busControl = ConfigureBus();

            await busControl.StartAsync();
            var scheduleSendEndPoint = await busControl.GetSendEndpoint(new Uri("rabbitmq://localhost/quartz"));
            var scheduledRecurringMessage = await scheduleSendEndPoint.ScheduleRecurringSend(
                     new Uri("rabbitmq://localhost/BMessage"),
                     new BSchedule("/15 * * * * ?"),
                     new BMessageImp());

            do
            {
                Console.WriteLine("Enter q1 to exit without cancel, q2 to exit with cancel");
                Console.Write("> ");
                string value = Console.ReadLine();

                if ("q1".Equals(value, StringComparison.OrdinalIgnoreCase))
                    break;

                if ("q2".Equals(value, StringComparison.OrdinalIgnoreCase))
                {
                    await scheduleSendEndPoint.CancelScheduledRecurringSend(scheduledRecurringMessage);
                    break;
                }

            }
            while (true);

            await busControl.StopAsync();
        }

        static IBusControl ConfigureBus()
        {
            return Bus.Factory.CreateUsingRabbitMq(cfg =>
            {
                var host = cfg.Host(new Uri("rabbitmq://localhost"), h =>
                {
                    h.Username("guest");
                    h.Password("guest");
                });
                cfg.ReceiveEndpoint(host, "BMessage", ep =>
                {
                    ep.Consumer<BConsumer>();
                    EndpointConvention.Map<BMessage>(ep.InputAddress);

                });
                cfg.UseInMemoryScheduler();
            });
        }
    }
    public class BSchedule : DefaultRecurringSchedule
    {
        public BSchedule(string cronExpression)
        {
            CronExpression = cronExpression;
        }
    }

    public interface BMessage
    {
    }
    public class BMessageImp : BMessage
    {
    }
    public class BConsumer : IConsumer<BMessage>
    {
        public Task Consume(ConsumeContext<BMessage> context)
        {
            Console.WriteLine($"Handled BMessage: {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
