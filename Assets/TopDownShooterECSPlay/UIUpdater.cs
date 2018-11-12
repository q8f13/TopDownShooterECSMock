using System.Collections;
using Unity.Collections;
using Unity.Entities;
using UnityEngine;
using UnityEngine.UI;

namespace Playground
{
	public class UIUpdater : ComponentSystem 
	{
		private Text _text;
		private Button _startBtn;

		public struct TimerData
		{
			public ComponentDataArray<GameTimer> Timer;
		}

		[Inject] TimerData _timer;

		struct PlayerData
		{
			public ComponentDataArray<Health> Health;
			public ComponentDataArray<PlayerInput> Input;
		}

		[Inject] PlayerData _player;

		public void GetElementRefs()
		{
			_text = GameObject.Find("Canvas/Text").GetComponent<Text>();
			_startBtn = GameObject.Find("Canvas/Start").GetComponent<Button>();
			_startBtn.onClick.AddListener(Bootloader.StartNew);
		}

		protected override void OnUpdate()
		{
			if(_player.Health.Length > 0)
			{
				// player alive
				_startBtn.gameObject.SetActive(false);
				_text.gameObject.SetActive(true);

				GameTimer gt = _timer.Timer[0];
				gt.Value = _timer.Timer[0].Value + Time.deltaTime;
				_timer.Timer[0] = gt;

				_text.text = $"Timer: {gt.Value.ToString("F2")} / Health: {_player.Health[0].Value}";
			}
			else
			{
				// player dead

				_startBtn.gameObject.SetActive(true);
				_text.gameObject.SetActive(false);
			}
		}
	}
}