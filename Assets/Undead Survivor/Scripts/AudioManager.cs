using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AudioManager : MonoBehaviour
{
    public static AudioManager instance;

    [Header("#BGM")]        // 배경음
    public AudioClip bgmClip;
    public float bgmVolume;
    AudioSource bgmPlayer;
    AudioHighPassFilter bgmEffect;

    [Header("#SFX")]        // 효과음
    public AudioClip[] sfxClips;
    public float sfxVolume;
    public int channels;    // 다량의 효과음을 낼 수 있도록 채널 개수 변수 선언
    AudioSource[] sfxPlayers;
    int channelIndex;       // 핸재 재생중인 채널의 번호를 담는 변수

    public enum Sfx { Dead, Hit, LevelUp = 3, Lose, Melee, Range = 7, Select, Win };

    private void Awake()
    {
        instance = this;
        Init();
    }

    private void Init()
    {
        // 배경음 플레이어 초기화
        GameObject bgmObject = new GameObject("BgmPlayer");
        bgmObject.transform.parent = transform;
        bgmPlayer = bgmObject.AddComponent<AudioSource>();
        bgmPlayer.playOnAwake = false;      // 시작시 바로 재생되는 것이 아니라 게임 시작시 재생되게 하기 위해 초기엔 비활성화
        bgmPlayer.loop = true;              // 배겨음악은 끊기지 않고 재생되어야 하기 때문에 무한반복 활성화
        bgmPlayer.volume = bgmVolume;       // 볼륨 설정
        bgmPlayer.clip = bgmClip;           // 재생할 음원 설정
        bgmEffect = Camera.main.GetComponent<AudioHighPassFilter>();

        // 효과음 플레이어 초기화
        GameObject sfxObject = new GameObject("SfxPlayer");
        sfxObject.transform.parent = transform;
        sfxPlayers = new AudioSource[channels]; // 초기화는 채널 갯수 만큼

        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            sfxPlayers[index] = sfxObject.AddComponent<AudioSource>();
            sfxPlayers[index].playOnAwake = false;
            sfxPlayers[index].volume = sfxVolume;

            // 레벨업 하는 함수 코드에서 AudioHighPassFilter를 이용해 배경음에 변화를 줍니다.
            // 하지만, 레벨업 사운드도 재생되기에 둘이 같이 영향을 받으면 레벨업 사운드가 이상하게 들리게 됩니다. 이를 방지하기 위해서
            // ListenerEffects에 해당되는 AudioHighPassFilter를 true로 변경해줍니다.(이는 유니티 에디터 AudioSource에 옵션으로 있습니다.)
            // 이는 HighFilter효과를 bypass 즉, 무시한다는 의미가 됩니다.
            // 이렇게 되면 배경음에는 효과가 먹히며, 그 외의 사운드를 재생하는 AudioSource효과음들은 정상적으로 재생됩니다.
            sfxPlayers[index].bypassListenerEffects = true;
        }
    }

    public void PlayBgm(bool isPlay)
    {
        if (isPlay)
        {
            bgmPlayer.Play();
        }
        else
        {
            bgmPlayer.Stop();
        }
    }

    public void EffectBgm(bool isPlay)
    {
        bgmEffect.enabled = isPlay;
    }

    public void PlaySfx(Sfx sfx)
    {
        for (int index = 0; index < sfxPlayers.Length; index++)
        {
            // sfxPlayers의 길이만큼 값을 나눔으로 인해서 배열의 길이를 벗어나는 것을 방지합니다.
            int loopIndex = (index + channelIndex) % sfxPlayers.Length;

            // 이미 재생중이라면 바로 다음 루프로 건너 뛰는 의미
            if (sfxPlayers[loopIndex].isPlaying)
                continue;

            // 효과음이 2개 이상인 경우 랜덤 인덱스를 활용
            int ranIndex = 0;
            if (sfx == Sfx.Hit || sfx == Sfx.Melee)
            {
                ranIndex = Random.Range(0, 2);
            }

            channelIndex = loopIndex;
            sfxPlayers[loopIndex].clip = sfxClips[(int)sfx + ranIndex];
            sfxPlayers[loopIndex].Play();
            break;  // 플레이 했으면 빠져나오기
        }
    }
}
