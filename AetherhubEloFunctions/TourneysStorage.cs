using Google.Cloud.Firestore;

namespace AetherhubEloFunctions;

public class TourneysStorage(FirestoreDb firestoreDb)
{
    public async Task WriteTourney(Tourney tourney)
    {
        var tourneyDocument = firestoreDb.Collection("tourneys").Document(tourney.Id.ToString("N"));
        await tourneyDocument.SetAsync(new Dictionary<string, object>
        {
            ["aetherhub_id"] = tourney.AetherhubId,
            ["date"] = tourney.Date,
            ["rounds"] = tourney.Rounds,
            ["communix"] = tourney.Communix,
        });
    }

    public async IAsyncEnumerable<Tourney> GetTourneys()
    {
        var collection = firestoreDb.Collection("tourneys").ListDocumentsAsync();
        await foreach (var document in collection)
        {
            var tourney = await document.GetSnapshotAsync();
            yield return new Tourney(
                Guid.Parse(tourney.Id),
                tourney.GetValue<int>("aetherhub_id"),
                tourney.GetValue<string>("communix"),
                tourney.GetValue<DateOnly>("date"),
                tourney.GetValue<Round[]>("rounds"));
        }
    }
}