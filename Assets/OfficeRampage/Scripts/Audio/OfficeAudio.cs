using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace OfficeRampage
{
    public sealed class OfficeAudio : MonoBehaviour
    {
        public float MusicVolume { get; set; } = .26f;
        public float EffectsVolume { get; set; } = .75f;
        public bool Muted { get; set; }
        private readonly Dictionary<string, AudioClip> clips = new Dictionary<string, AudioClip>();
        private readonly AudioSource[] music = new AudioSource[2];
        private readonly AudioSource[] effects = new AudioSource[16];
        private readonly float[] weights = new float[2];
        private readonly float[] trackGain = { 1, 1 };
        private Coroutine transition;
        private int activeMusic, voice, impactFrame = -1, impactsThisFrame;
        private float duckUntil;
        private bool paused;

        private void Awake()
        {
            for (int i = 0; i < music.Length; i++)
            {
                music[i] = Source("Music " + i);
                music[i].loop = true;
                music[i].priority = 32;
            }
            for (int i = 0; i < effects.Length; i++) effects[i] = Source("Effect " + i);
            string[] keys = { "wood0", "wood1", "wood2", "metal0", "metal1", "metal2", "glass0", "glass1",
                "thud", "wall", "combo", "door", "win", "lose", "swing", "shot", "rocket", "explosion", "machinegun", "shotgun", "thunder" };
            foreach (string key in keys) Load("Sfx/" + key);
            for (int i = 1; i <= 5; i++) Load("Music/office" + i);
            Load("Music/boss");
        }

        private AudioSource Source(string label)
        {
            var source = new GameObject(label).AddComponent<AudioSource>();
            source.transform.SetParent(transform, false);
            source.playOnAwake = false;
            source.dopplerLevel = 0;
            source.rolloffMode = AudioRolloffMode.Linear;
            source.minDistance = 5;
            source.maxDistance = 35;
            return source;
        }

        private AudioClip Load(string key)
        {
            if (clips.TryGetValue(key, out var cached)) return cached;
            string replacement=null;
            switch(key)
            {
                case "Sfx/shot": break;
                case "Sfx/machinegun": key="Sfx/shot"; break;
                case "Sfx/shotgun": key="Sfx/shot"; break;
                case "Sfx/rocket": replacement="rocket"; break;
                case "Sfx/explosion": replacement="explosion"; break;
                case "Sfx/thunder": replacement="thunder"; break;
                case "Sfx/thud": replacement="impact7"; break;
                case "Sfx/wood0": replacement="impact1"; break;
                case "Sfx/wood1": replacement="impact2"; break;
                case "Sfx/wood2": replacement="impact3"; break;
                case "Sfx/metal0": replacement="impact4"; break;
                case "Sfx/metal1": replacement="impact5"; break;
                case "Sfx/metal2": replacement="impact6"; break;
            }
            var clip = Resources.Load<AudioClip>(replacement==null ? "OfficeAudio/"+key : "OfficeAudio/CombatRefresh/"+replacement);
            if (clip == null) Debug.LogError("Office Rampage audio missing: " + key);
            clips[key] = clip;
            return clip;
        }

        public void ResetRun()
        {
            if (transition != null) StopCoroutine(transition);
            transition = null;
            paused = false;
            foreach (var source in music) source.Stop();
            foreach (var source in effects) source.Stop();
            weights[0] = weights[1] = 0;
            activeMusic = 0; duckUntil = 0;
            ChangeRoom(1);
        }

        public void ChangeRoom(int room)
        {
            var clip = Load(room == 6 ? "Music/boss" : "Music/office" + Mathf.Clamp(room, 1, 5));
            if (clip == null) return;
            if (transition != null) StopCoroutine(transition);
            int next = 1 - activeMusic;
            music[next].Stop();
            music[next].clip = clip;
            // Match the measured RMS levels of the original tracks without modifying the assets.
            float[] gains = { 1f, .56f, .47f, .75f, .95f, 1.1f };
            trackGain[next] = gains[Mathf.Clamp(room, 1, 6) - 1];
            music[next].volume = 0;
            weights[next] = 0;
            music[next].Play();
            if (paused) music[next].Pause();
            transition = StartCoroutine(Crossfade(activeMusic, next));
            activeMusic = next;
        }

        private IEnumerator Crossfade(int old, int next)
        {
            float initial = weights[old], elapsed = 0;
            while (elapsed < 1.2f)
            {
                if (!paused)
                {
                    elapsed += Time.unscaledDeltaTime;
                    float t = Mathf.Clamp01(elapsed / 1.2f);
                    weights[old] = initial * (1 - t);
                    weights[next] = t;
                }
                yield return null;
            }
            weights[old] = 0; weights[next] = 1;
            music[old].Stop(); transition = null;
        }

        private void Update()
        {
            float duck = Time.unscaledTime < duckUntil ? .65f : 1;
            for (int i = 0; i < music.Length; i++)
            {
                music[i].mute = Muted;
                music[i].volume = Mathf.MoveTowards(music[i].volume, weights[i] * trackGain[i] * MusicVolume * duck, Time.unscaledDeltaTime * 1.5f);
            }
            foreach (var source in effects) source.mute = Muted;
        }

        public void SetPaused(bool value)
        {
            if (paused == value) return;
            paused = value;
            foreach (var source in music) { if (value) source.Pause(); else source.UnPause(); }
            foreach (var source in effects) { if (value) source.Pause(); else source.UnPause(); }
        }

        public void Play(string key, float volume = 1, float pitch = 1, Vector3? position = null)
        {
            if (paused) return;
            var clip = Load("Sfx/" + key);
            if (clip == null) return;
            // A bounded voice pool prevents runaway layering during explosions.
            var source = effects[voice++ % effects.Length];
            source.Stop();
            source.clip = clip;
            source.spatialBlend = position.HasValue ? .75f : 0;
            if (position.HasValue) source.transform.position = position.Value;
            source.pitch = pitch;
            source.volume = EffectsVolume * volume;
            source.mute = Muted;
            source.Play();
        }

        public void Weapon(int index)
        {
            string key=index<=1 || index==4 ? "swing" : index==3 ? "machinegun" : index==5 ? "shotgun" : index>=6 ? "rocket" : "shot";
            Play(key,index<=1?.35f:.65f,Random.Range(.93f,1.04f));
            duckUntil = Time.unscaledTime + .18f;
        }

        public void Impact(int material, bool broken, Vector3 position)
        {
            if (impactFrame != Time.frameCount) { impactFrame = Time.frameCount; impactsThisFrame = 0; }
            if (++impactsThisFrame > 2) return;
            string key = material == 0 ? "wood" + Random.Range(0, 3) : "metal" + Random.Range(0, 3);
            Play(key, broken ? .65f : .4f, Random.Range(.88f, 1.1f), position);
            if (broken && (material == 1 || material == 2))
                Play("glass" + Random.Range(0, 2), .32f, Random.Range(.9f, 1.08f), position);
            duckUntil = Time.unscaledTime + .12f;
        }

        public void Finish(bool victory)
        {
            if (transition != null) StopCoroutine(transition);
            transition = null;
            weights[0] = weights[1] = 0;
            foreach (var source in effects) source.Stop();
            Play(victory ? "win" : "lose", .65f);
        }
    }
}
