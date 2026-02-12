using System.Threading.Tasks;
using UnityEngine;
using Unity.Services.Core;
using Unity.Services.Authentication;

public class AccountManager : MonoBehaviour
{
    // SignInAnonymouslyAsync() 메서드가 캐시된 플레이어의 기존 자격 증명을 복구합니다.
    // 플레이어 로그인 정보가 존재하지 않는 경우, SignInAnonymouslyAsync() 메서드가 새 익명 플레이어를 생성합니다.
    async void Awake()
    {
        await SignInAnonymouslyAsync();
    }

    async Task SignInAnonymouslyAsync()
    {
        try
        {
            await UnityServices.InitializeAsync();
            await AuthenticationService.Instance.SignInAnonymouslyAsync();
            Debug.Log("Signed in Anonymously Succeeded");

            Debug.Log($"PlayerID: {AuthenticationService.Instance.PlayerId}");
        }
        catch (AuthenticationException ex)
        {
            Debug.LogError($"Sign in failed: {ex}");
        }
        catch (RequestFailedException ex)
        {
            Debug.LogError($"Sign in failed: {ex}");
        }
    }
}
