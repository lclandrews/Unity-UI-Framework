using System.Collections.Generic;

using UnityEngine;

namespace UIFramework
{
    public class AnimationSystemRunner : MonoBehaviour
    {
        private static AnimationSystemRunner _instance = null;
        private static bool _destroyed = false;

        private static HashSet<AnimationPlayer> _playerHashSet = new HashSet<AnimationPlayer>();
        private static List<AnimationPlayer> _playerList = new List<AnimationPlayer>();

        public static void AddPlayer(AnimationPlayer player)
        {
            if(ValidateInstance())
            {
                if (!_playerHashSet.Contains(player))
                {
                    _playerHashSet.Add(player);
                    _playerList.Add(player);
                }
            }                        
        }

        private static bool ValidateInstance()
        {
            if (!_destroyed && _instance == null)
            {
                GameObject go = new GameObject("AnimationSystemRunner");
                DontDestroyOnLoad(go);
                _instance = go.AddComponent<AnimationSystemRunner>();
            }

            return _instance != null;            
        }

        private void LateUpdate()
        {
            for(int i = _playerList.Count - 1; i >= 0; i--)
            {
                AnimationPlayer player = _playerList[i];
                player.Update();
                if(!player.IsPlaying)
                {
                    _playerList.RemoveAt(i);
                    _playerHashSet.Remove(player);
                }
            }
        }

        private void OnDestroy()
        {
            if(_instance == this)
            {
                _instance = null;
                _destroyed = true;
                _playerHashSet.Clear();
                _playerList.Clear();
            }            
        }
    }
}