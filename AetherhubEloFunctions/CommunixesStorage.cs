using Google.Cloud.Firestore;

namespace AetherhubEloFunctions;

public class CommunixesStorage(FirestoreDb firestoreDb)
{
    public async Task<IEnumerable<Communix>> GetAll()
    {
        var documents = firestoreDb
            .Collection("communixes")
            .ListDocumentsAsync();
        var communixes = new List<Communix>();
        await foreach (var document in documents)
        {
            communixes.Add(ToModel(await document.GetSnapshotAsync()));
        }

        return communixes;
    }

    private static Communix ToModel(DocumentSnapshot document)
    {
        return new Communix(
            document.Id,
            document.GetValue<string>("name"),
            (DayOfWeek)document.GetValue<int>("weekday"),
            document.GetValue<string>("aliases")?.Split(',').ToArray() ?? []);
    }
}