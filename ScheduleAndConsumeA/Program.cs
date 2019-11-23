using MassTransit;
using MassTransit.Scheduling;
using Quartz;
using System;
using System.Threading.Tasks;

namespace ScheduleAndConsumeA
{
    class Program
    {
        static async Task Main(string[] args)
        {
            var busControl = ConfigureBus();

            await busControl.StartAsync();
            var scheduleSendEndPoint = await busControl.GetSendEndpoint(new Uri("rabbitmq://localhost/quartz"));
            var scheduledRecurringMessage = await scheduleSendEndPoint.ScheduleRecurringSend(
                     new Uri("rabbitmq://localhost/AMessage"),
                     new ASchedule("/15 * * * * ?"),
                     new AMessageImp());

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
                cfg.ReceiveEndpoint(host, "AMessage", ep =>
                {
                    ep.Consumer<AConsumer>();
                    EndpointConvention.Map<AMessage>(ep.InputAddress);

                });
                cfg.UseInMemoryScheduler();
            });
        }
    }
    public class ASchedule : DefaultRecurringSchedule
    {
        public ASchedule(string cronExpression)
        {
            CronExpression = cronExpression;
        }
    }

    public interface AMessage
    {
    }
    public class AMessageImp : AMessage
    {
    }
    public class AConsumer : IConsumer<AMessage>
    {
        public Task Consume(ConsumeContext<AMessage> context)
        {
            Console.WriteLine($"Handled AMessage: {DateTime.Now}");
            return Task.CompletedTask;
        }
    }
}
