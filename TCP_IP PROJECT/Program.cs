using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;

class RockPaperScissorsServer
{
    private static int clientCounter = 0; // Counter to generate unique client IDs
    static void Main(string[] args)
    {
        TcpListener server = new TcpListener(IPAddress.Any, 5000);
        server.Start();
        Console.WriteLine("Server started on port 5000...");

        while (true)
        {
            TcpClient client = server.AcceptTcpClient();
            clientCounter++;
            int clientId = clientCounter; // Assign a unique ID to the client
            Console.WriteLine($"Client {clientId} connected.");
            Thread clientThread = new Thread(() => HandleClient(client, clientId));
            clientThread.Start();
        }
    }

    static void HandleClient(TcpClient client, int clientId)
    {
        NetworkStream stream = client.GetStream();
        byte[] buffer = new byte[1024];
        string[] choices = { "Rock", "Paper", "Scissors" };
        Random random = new Random();

        int clientWins = 0;
        int serverWins = 0;

        try
        {
            while (true)
            {
                // Receive client's choice
                int bytesRead = stream.Read(buffer, 0, buffer.Length);
                if (bytesRead == 0) break;
                string clientChoice = Encoding.ASCII.GetString(buffer, 0, bytesRead).Trim().ToLower();

                if (clientChoice.Equals("exit"))
                {
                    string finalWinner = clientWins > serverWins ? ("client"+clientId) :(serverWins > clientWins?"server" : "No winner");

                    string finalScore = $"Game Over.\nFinal Scores:\nClient Wins: {clientWins}\nServer Wins: {serverWins}\nFinal Winner: {finalWinner}\n";
                    stream.Write(Encoding.ASCII.GetBytes(finalScore), 0, finalScore.Length);
                    Console.WriteLine($"Client {clientId} disconnected. Final Score: Client {clientWins} - {serverWins} Server");
                    break;
                }

                if (!int.TryParse(clientChoice, out int clientMove) || clientMove < 1 || clientMove > 3)
                {
                    string invalidMsg = "Invalid choice, please try again\n";
                    stream.Write(Encoding.ASCII.GetBytes(invalidMsg), 0, invalidMsg.Length);
                    continue;
                }

                string serverChoice = choices[random.Next(3)];
                clientChoice = choices[clientMove - 1];
                string result = DetermineWinner(clientChoice, serverChoice);

                if (result.Contains("You win")) clientWins++;
                else if (result.Contains("Server wins")) serverWins++;

                string RoundRes = $"Client {clientId}: {clientChoice} and Server: {serverChoice}\n{result}\nCurrent Score: Client {clientWins} - {serverWins} Server\n";
                stream.Write(Encoding.ASCII.GetBytes(RoundRes), 0, RoundRes.Length);
                
            }
        }
        finally
        {
            client.Close();
        }
    }

    static string DetermineWinner(string  clientChoice, string serverChoice)
    {
        
        if (clientChoice == serverChoice)
        {
            return "It's a tie!";
        }
            
        if ((clientChoice == "Rock" && serverChoice == "Scissors") ||
            (clientChoice == "Paper" && serverChoice == "Rock") ||
            (clientChoice == "Scissors" && serverChoice == "Paper"))
            return "You win!";
        return "Server wins!";
    }
}