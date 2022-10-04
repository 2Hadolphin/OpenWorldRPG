using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Zenject
{
    public interface IRegister
    {
        void Register(DiContainer container);
    }
}

