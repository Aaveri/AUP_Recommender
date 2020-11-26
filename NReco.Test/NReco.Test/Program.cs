using CsvHelper;
using NReco.CF.Taste.Impl.Model.File;
using NReco.CF.Taste.Impl.Neighborhood;
using NReco.CF.Taste.Impl.Recommender;
using NReco.CF.Taste.Impl.Similarity;
using NReco.CF.Taste.Recommender;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace NReco.Test
{
    public class MovieRecord
    {
        public MovieRecord(int movieId, string title)
        {
            this.movieId = movieId;
            this.title = title;
        }

        public int movieId { get; set; }
        public string title { get; set; }
    }

    class Program
    {
        static void Main(string[] args)
        {


            // load movies
            var movies = new List<MovieRecord>();
            using (TextReader reader = File.OpenText(@"data/movies.csv"))
            {
                CsvReader csv = new CsvReader(reader);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.MissingFieldFound = null;
                while (csv.Read())
                {
                    movies.Add(csv.GetRecord<MovieRecord>());
                }
            }

            // load users
            List<int> users = new List<int>();
            using (TextReader reader = File.OpenText(@"data/ratings.csv"))
            {
                CsvReader csv = new CsvReader(reader);
                csv.Configuration.Delimiter = ",";
                csv.Configuration.MissingFieldFound = null;
                while (csv.Read())
                {
                    if (int.TryParse(csv.GetField(0), out int id))
                    {
                        if (!users.Contains(id))
                        {
                            users.Add(id);
                        }
                    }
                }
            }


            // load data model
            var model = new FileDataModel("data/ratings.csv", false, FileDataModel.DEFAULT_MIN_RELOAD_INTERVAL_MS, false);


            //init values
            string similarityType = "Cosine Similarity";
            string recommenderType = "Userbased";
            int neighborhoodSize = 125;
            int resultCount = 10;
            List<int> movieIdPredList = new List<int>();
            List<int> userIdPredList = new List<int>();

            //default recommender
            AbstractSimilarity similarity = new UncenteredCosineSimilarity(model);
            var neighborhood = new NearestNUserNeighborhood(neighborhoodSize, similarity, model);
            AbstractRecommender recommender = new GenericUserBasedRecommender(model, neighborhood, similarity);

     
            string input = "";

            while (input != "X")
            {
                Console.WriteLine();
                Console.WriteLine("Was möchten Sie tun?");
                Console.WriteLine("I - Umschalten auf Item-based Ansatz");
                Console.WriteLine("U - Umschalten auf User-based Ansatz");
                Console.WriteLine("DE - Umschalten auf Cosine Similarity");
                Console.WriteLine("PE - Umschalten auf Pearson Correlation");
                Console.WriteLine("N - Anzahl der Nachbarn festlegen");
                Console.WriteLine("C - Konfiguration neu erstellen und Datendatei neu lesen");
                Console.WriteLine("O - Aktuelle Konfiguration ausgeben");
                Console.WriteLine("E - Empfehlung durchführen");
                Console.WriteLine("P - Prediction für spezifischen Film durchführen");
                Console.WriteLine("UADD - Benutzer zur Liste für Vorschläge hinzufügen");
                Console.WriteLine("MADD - Film zur Filmliste hinzufügen");
                Console.WriteLine("UCLEAR - Benutzerliste leeren");
                Console.WriteLine("MCLEAR - Filmliste leeren");
                Console.WriteLine("X - Programm beenden");

                input = Console.ReadLine();

                if (input == "I")
                {
                    recommenderType = "Itembased";
                    recommender = new GenericItemBasedRecommender(model, similarity);
                    Console.WriteLine("Recommendertyp auf \"itembased\" geändert.");
                }

                else if (input == "U")
                {
                    recommenderType = "Userbased";
                    recommender = new GenericUserBasedRecommender(model, neighborhood, similarity);
                    Console.WriteLine("Recommendertyp auf \"userbased\" geändert.");
                }

                else if (input == "DE")
                {
                    similarityType = "Cosine Similarity";
                    similarity = new UncenteredCosineSimilarity(model);
                    Console.WriteLine("Similaritytyp auf \"Cosine Similarity\" geändert.");
                }
                else if (input == "PE")
                {
                    similarityType = "Pearson Correlation Similarity";
                    similarity = new PearsonCorrelationSimilarity(model);
                    Console.WriteLine("Similaritytyp auf \"Pearson Correlation Similarity\" geändert.");
                }
                else if (input == "N")
                {
                    Console.WriteLine("Wie groß soll die Nachbarschaft sein?");
                    string neighborhoodSizeInput = Console.ReadLine();
                    if (int.TryParse(neighborhoodSizeInput, out int neighborhoodSizeParsed))
                    {
                        neighborhoodSize = neighborhoodSizeParsed;
                        neighborhood = new NearestNUserNeighborhood(neighborhoodSize, similarity, model);
                        Console.WriteLine("Nachbarschaftsgröße auf " + neighborhoodSize + " geändert.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                        Console.WriteLine("Die Nachbarschaftsgröße bleibt bei " + neighborhoodSize + ".");
                    }
                }
                else if (input == "C")
                {
                    Console.WriteLine("Wie groß soll die Nachbarschaft sein?");
                    string neighborhoodSizeInput = Console.ReadLine();
                    if (int.TryParse(neighborhoodSizeInput, out int neighborhoodSizeParsed))
                    {
                        neighborhoodSize = neighborhoodSizeParsed;
                        neighborhood = new NearestNUserNeighborhood(neighborhoodSize, similarity, model);
                        Console.WriteLine("Nachbarschaftsgröße auf " + neighborhoodSize + " geändert.");
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                        Console.WriteLine("Die Nachbarschaftsgröße bleibt bei " + neighborhoodSize + ".");
                    }

                    Console.WriteLine("Wieviele Filme sollen vorgeschlagen werden?");

                    string resultCountInput = Console.ReadLine();
                    if (int.TryParse(resultCountInput, out int resultCountParsed))
                    {
                        resultCount = resultCountParsed;
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                        Console.WriteLine("Die Anzahl der vorgeschlagenen Filme bleibt bei " + resultCount + ".");
                    }

                    foreach (int userId in userIdPredList)
                    {
                        Console.WriteLine("Vorgeschlagene Filme für User mit UserId " + userId);
                        IList<IRecommendedItem> recommendedItems = recommender.Recommend(userId, resultCount);

                        foreach (var recItem in recommendedItems)
                        {
                            Console.WriteLine("Item: " + recItem.GetItemID() + " (" + movies.First(x => x.movieId == recItem.GetItemID()).title + ") ===> " + recItem.GetValue());
                        }
                        Console.WriteLine();

                    }

                    // load movies
                    movies = new List<MovieRecord>();
                    using (TextReader reader = File.OpenText(@"data/movies.csv"))
                    {
                        CsvReader csv = new CsvReader(reader);
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.MissingFieldFound = null;
                        while (csv.Read())
                        {
                            movies.Add(csv.GetRecord<MovieRecord>());
                        }
                    }

                    // load users
                    users = new List<int>();
                    using (TextReader reader = File.OpenText(@"data/ratings.csv"))
                    {
                        CsvReader csv = new CsvReader(reader);
                        csv.Configuration.Delimiter = ",";
                        csv.Configuration.MissingFieldFound = null;
                        while (csv.Read())
                        {
                            if (int.TryParse(csv.GetField(0), out int id))
                            {
                                if (!users.Contains(id))
                                {
                                    users.Add(id);
                                }
                            }
                        }
                    }

                    model = new FileDataModel("data/ratings.csv", false, FileDataModel.DEFAULT_MIN_RELOAD_INTERVAL_MS, false);

                    if(similarityType == "Cosine Similarity")
                    {
                        similarity = new UncenteredCosineSimilarity(model);
                    }
                    else
                    {
                        similarity = new PearsonCorrelationSimilarity(model);
                    }

                    neighborhood = new NearestNUserNeighborhood(neighborhoodSize, similarity, model);

                    if(recommenderType == "Itembased")
                    {
                        recommender = new GenericItemBasedRecommender(model, similarity);
                    }
                    else
                    {
                        recommender = new GenericUserBasedRecommender(model, neighborhood, similarity);
                    }
                }
                else if (input == "O")
                {
                    int countPredUser = userIdPredList.Count;
                    int countPredMovies = movieIdPredList.Count;

                    Console.WriteLine("-----------------------------------------------");
                    Console.WriteLine("Recommender Typ: " + recommenderType);
                    Console.WriteLine("Similarity Typ: " + similarityType);
                    Console.WriteLine("Nachbarschaftsgröße: " + neighborhoodSize);
                    Console.WriteLine("Anzahl Ergebnisse: " + resultCount);
                    Console.WriteLine("Anzahl Benutzer: " + countPredUser);
                    Console.WriteLine("Anzahl vorgeschlagene Filme: " + countPredMovies);
                    Console.WriteLine("-----------------------------------------------");
                }
                else if (input == "E")
                {
                    Console.WriteLine("Wieviele Filme sollen vorgeschlagen werden?");

                    string resultCountInput = Console.ReadLine();
                    if (int.TryParse(resultCountInput, out int resultCountParsed))
                    {
                        resultCount = resultCountParsed;
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                        Console.WriteLine("Die Anzahl der vorgeschlagenen Filme bleibt bei " + resultCount + ".");
                    }

                    foreach(int userId in userIdPredList)
                    {
                        Console.WriteLine("Vorgeschlagene Filme für User mit UserId " + userId);
                        IList<IRecommendedItem> recommendedItems = recommender.Recommend(userId, resultCount);

                        foreach (var recItem in recommendedItems)
                        {
                            Console.WriteLine("Item: " + recItem.GetItemID() + " (" + movies.First(x => x.movieId == recItem.GetItemID()).title + ") ===> " + recItem.GetValue());
                        }
                        Console.WriteLine();

                    }



                }
                else if (input == "P")
                {
                    foreach(int userId in userIdPredList)
                    {
                        Console.WriteLine("Prediction für User mit Id" + userId + ":");
                        foreach(int movieId in movieIdPredList)
                        {
                            float predictionValue = recommender.EstimatePreference(userId, movieId);
                            Console.WriteLine(movieId + " -> " + movies.First(x => x.movieId == movieId).title + " ===> " + predictionValue);
                        }
                        Console.WriteLine();
                    }
                }
                else if (input == "UADD")
                {
                    Console.WriteLine("Welcher Benutzer (Id) soll zur Liste für Vorschläge hinzugefügt werden?");

                    string userIdInput = Console.ReadLine();
                    if (int.TryParse(userIdInput, out int userIdParsed))
                    {
                        if (users.Contains(userIdParsed))
                        {
                        userIdPredList.Add(userIdParsed);
                        }
                        else
                        {
                            Console.WriteLine("Error: UserId existiert nicht.");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                    }
                }
                else if (input == "MADD")
                {
                    Console.WriteLine("Welcher Film (Id) soll zur Liste für Vorschläge hinzugefügt werden?");
                    string movieIdInput = Console.ReadLine();
                    if (int.TryParse(movieIdInput, out int movieIdParsed))
                    {
                        bool movieIdFound = false;
                        foreach (MovieRecord movie in movies)
                        {
                            if (movie.movieId == movieIdParsed)
                            {
                                movieIdPredList.Add(movieIdParsed);
                                movieIdFound = true;
                                break;
                            }
                        }
                        if (!movieIdFound)
                        {
                            Console.WriteLine("Error: movieId existiert nicht. Es wurde kein Film zur Liste hinzugefügt");
                        }
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                    }
                }
                else if (input == "UCLEAR")
                {
                    userIdPredList.Clear();
                }
                else if (input == "MCLEAR")
                {
                    movieIdPredList.Clear();
                }
            }

            Console.WriteLine("Programm beendet");

            Console.ReadLine();   
        }
    }
}
