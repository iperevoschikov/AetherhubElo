using Google.Cloud.Firestore;

namespace AetherhubEloFunctions;

public class UsersStorage(FirestoreDb firestoreDb)
{
    public async Task<(UserState, string? data)> GetUserState(long userId)
    {
        var user = await GetDocumentReference(userId).GetSnapshotAsync();

        if (user.Exists && user.TryGetValue<UserState>("state", out var state))
            return user.TryGetValue<string>("data", out var data)
                ? (state, data)
                : (state, null);

        return (UserState.Default, null);
    }

    public async Task SetUserCommunix(long userId, string communix)
    {
        var userDocumentReference = GetDocumentReference(userId);
        await userDocumentReference.SetAsync(
            new Dictionary<string, object?>
            {
                ["communix"] = communix,
            },
            SetOptions.MergeAll);
    }

    public async Task<string?> GetUserCommunix(long userId)
    {
        var user = await GetDocumentReference(userId).GetSnapshotAsync();

        if (user.Exists && user.TryGetValue<string>("communix", out var communix))
            return communix;

        return null;
    }

    public async Task SetUserState(long userId, UserState state, string? data = null)
    {
        var userDocumentReference = GetDocumentReference(userId);
        await userDocumentReference.SetAsync(
            new Dictionary<string, object?>
            {
                ["state"] = state,
                ["data"] = data,
            },
            SetOptions.MergeAll);
    }

    private DocumentReference GetDocumentReference(long userId)
    {
        return firestoreDb
            .Collection("users")
            .Document(userId.ToString());
    }
}