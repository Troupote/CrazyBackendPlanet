using NATS.Client;
using System;
using System.Threading;

namespace NatsPlayground;

class Program
{
    static void Main(string[] args)
    {
        bool continueExecution = true;

        while (continueExecution)
        {
            try
            {
                Console.WriteLine("Tentative de connexion au serveur NATS...");

                // Connexion au serveur NATS (par défaut localhost:4222)
                var factory = new ConnectionFactory();
                using var connection = factory.CreateConnection();
                Console.WriteLine("✓ Connexion réussie au serveur NATS!");

                // S'abonner à un sujet
                var subscription = connection.SubscribeAsync("game.events", (sender, args) =>
                {
                    try
                    {
                        var message = System.Text.Encoding.UTF8.GetString(args.Message.Data);
                        Console.WriteLine($"Reçu: {message}");
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"❌ Erreur lors du traitement du message: {ex.Message}");
                    }
                });

                Console.WriteLine("✓ Abonnement au sujet 'game.events' réussi!");

                // Publier un message
                byte[] data = System.Text.Encoding.UTF8.GetBytes("Hello NATS!");
                connection.Publish("game.events", data);

                Console.WriteLine("✓ Message publié avec succès!");

                // Garder l'application en vie
                Console.WriteLine("\nAppuyez sur 'q' pour quitter ou une autre touche pour continuer...");
                var key = Console.ReadKey(true);

                if (key.KeyChar == 'q' || key.KeyChar == 'Q')
                {
                    continueExecution = false;
                }

                subscription.Unsubscribe();
                Console.WriteLine("✓ Déconnexion réussie!");
            }
            catch (NATSConnectionException ex)
            {
                Console.WriteLine($"❌ Erreur de connexion NATS: {ex.Message}");
                continueExecution = Retry("Impossible de se connecter au serveur NATS.");
            }
            catch (NATSTimeoutException ex)
            {
                Console.WriteLine($"❌ Timeout connexion NATS: {ex.Message}");
                continueExecution = Retry("Timeout lors de la connexion au serveur NATS.");
            }
            catch (NATSException ex)
            {
                Console.WriteLine($"❌ Erreur NATS: {ex.Message}");
                continueExecution = Retry("Une erreur NATS s'est produite.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ Erreur inattendue: {ex.Message}");
                Console.WriteLine($"Détails: {ex.StackTrace}");
                continueExecution = Retry("Une erreur inattendue s'est produite.");
            }
        }

        Console.WriteLine("Au revoir!");

        Thread.Sleep(2000);
    }

    private static bool Retry(string messageErreur)
    {
        Console.WriteLine($"\n{messageErreur}");
        Console.WriteLine("Voulez-vous réessayer? (o/n): ");

        while (true)
        {
            var reponse = Console.ReadKey(true);

            if (reponse.KeyChar == 'o' || reponse.KeyChar == 'O')
            {
                Console.WriteLine("Nouvelle tentative...\n");
                return true;
            }
            else if (reponse.KeyChar == 'n' || reponse.KeyChar == 'N')
            {
                return false;
            }
            else
            {
                Console.WriteLine("Veuillez répondre par 'o' (oui) ou 'n' (non): ");
            }
        }
    }
}