#define UniTask

using UnityEngine;
using Return;

public class AudioSourcePool : SwitchPool<AudioSource>
{
    public AudioSourcePool(int capacity,bool coroutineInstance = true) : base(capacity) 
    {
        CoroutineInstance = coroutineInstance;
    }

    public AudioSource Template;

    public bool CoroutineInstance;

    public override void Init()
    {

    }

    public override AudioSource Get()
    {
        var source= base.Get();
        source.enabled = true;
        return source;
    }

    public override void Reset(int index)
    {
        base.Reset(index);
        
        var source = Values[index];
        
        if (source.isPlaying)
            source.Stop();

        source.enabled = false;
    }

    class PoolLoader : ICoroutineDelegate
    {
        public AudioSourcePool Pool;

        int sn=0;
        bool finish;
        
        public void Execute()
        {
            Pool.Cookies[sn] = true;
            Pool.Values[sn]=GameObject.Instantiate(Pool.Template);
            sn++;
            finish = sn == Pool.Capacity;
        }

        public bool Finish()
        {
            return finish;
        }
    }
}

