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

            // load data model
            var model = new FileDataModel("data/ratings.csv", false, FileDataModel.DEFAULT_MIN_RELOAD_INTERVAL_MS, false);


            //init values
            string similarityType = "Cosine Similarity";
            string recommenderType = "Userbased";
            int neighborhoodSize = 125;
            int userId = 72;
            int resultCount = 10;
            int predictionMovieId = 1374;

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
                    //Todo: Konfiguration??? neu erstellen und Datendatei neu lesen
                }
                else if (input == "O")
                {
                    int countUser = 0; //Todo: Anzahl User zählen
                    int countMovies = movies.Count;

                    Console.WriteLine("-----------------------------------------------");
                    Console.WriteLine("Recommender Typ: " + recommenderType);
                    Console.WriteLine("Similarity Typ: " + similarityType);
                    Console.WriteLine("Nachbarschaftsgröße: " + neighborhoodSize);
                    Console.WriteLine("Anzahl Ergebnisse: " + resultCount);
                    Console.WriteLine("User Id: " + userId);
                    Console.WriteLine("Prediction Movie Id: " + predictionMovieId);
                    Console.WriteLine("Anzahl Benutzer: " + countUser);
                    Console.WriteLine("Anzahl Filme: " + countMovies);
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
                    Console.WriteLine("Vorgeschlagene Filme:");
                    IList<IRecommendedItem> recommendedItems = recommender.Recommend(userId, resultCount);

                    foreach (var recItem in recommendedItems)
                    {
                        Console.WriteLine("Item: " + recItem.GetItemID() + " (" + movies.First(x => x.movieId == recItem.GetItemID()).title + ") ===> " + recItem.GetValue());
                    }

                }
                else if (input == "P")
                {
                    Console.WriteLine("Für welche Movie Id soll eine Prediction ausgegeben werden?");
                    string predictionMovieIdInput = Console.ReadLine();
                    if (int.TryParse(predictionMovieIdInput, out int predictionMovieIdParsed))
                    {
                        //Todo: Testen, ob movieId in datenbank vorhanden ist
                        predictionMovieId = predictionMovieIdParsed;
                    }
                    else
                    {
                        Console.WriteLine("Error: Es wurde keine Zahl eingegeben.");
                        Console.WriteLine("Es wird eine Prediction für die zuletzt eingegebene Movie Id " + predictionMovieId + " berechnet.");
                    }

                    var predictionValue = recommender.EstimatePreference(userId, predictionMovieId);
                    Console.WriteLine("Prediction: " + predictionMovieId + " (" + movies.First(x => x.movieId == predictionMovieId).title + " ===> " + predictionValue);
                }
                else if (input == "UADD")
                {
                    //Todo: Benutzer zur Liste für Vorschläge hinzufügen
                }
                else if (input == "MADD")
                {
                    //Todo: Film zur Filmliste für Prediction hinzufügen
                }
                else if (input == "UCLEAR")
                {
                    //Todo: Benutzerliste leeren
                }
                else if (input == "MCLEAR")
                {
                    //Todo: Filmliste leeren
                }
            }

            Console.WriteLine("Programm wird beendet");

            Console.ReadLine();
            
        }
    }
}
