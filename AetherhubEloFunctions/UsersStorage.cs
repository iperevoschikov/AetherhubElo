using Google.Cloud.Firestore;

namespace AetherhubEloFunctions;

public class UsersStorage(FirestoreDb firestoreDb)
{
    public async Task<UserState> GetUserState(long userId)
    {
        var user = await GetDocumentReference(userId).GetSnapshotAsync();

        if (user.Exists && user.TryGetValue<UserState>("state", out var state))
            return state;

        return UserState.Default;
    }

    public async Task SetUserState(long userId, UserState state)
    {
        var userDocumentReference = GetDocumentReference(userId);
        await userDocumentReference.SetAsync(new Dictionary<string, object>
        {
            ["state"] = state,
        });
    }

    private DocumentReference GetDocumentReference(long userId)
    {
        return firestoreDb
            .Collection("users")
            .Document(userId.ToString());
    }
}