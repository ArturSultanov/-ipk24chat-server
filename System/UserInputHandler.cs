using System;
using System.Threading;
using System.Threading.Tasks;

namespace ipk24chat_server.System;

public class UserInputHandler
{
    public async Task StartListeningForCommandsAsync(CancellationToken cancellationToken, Action requestCancel)
    {
        while (!cancellationToken.IsCancellationRequested)
        {
            var inputTask = Task.Run(Console.ReadLine, cancellationToken);
            var cancelTask = Task.Delay(Timeout.Infinite, cancellationToken);

            var completedTask = await Task.WhenAny(inputTask, cancelTask);

            if (completedTask == cancelTask)
            {
                break; // Cancellation requested
            }

            var input = await inputTask; // Get the result of Console.ReadLine
            if (input == null || input.Equals("^D")) // Check for EOF
            {
                if (!cancellationToken.IsCancellationRequested) requestCancel();
                break;
            }
        }
    }
}
