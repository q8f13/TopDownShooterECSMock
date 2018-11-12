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
		private Text _finalScore;
		private Button _startBtn;

		private string _txtBuffer;

		private bool _lastPlayerAlive = false;

		public struct TimerData
		{
			public ComponentDataArray<GameTimer> Timer;
			public ComponentDataArray<SpawnerState> State;
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
			_finalScore = GameObject.Find("Canvas/Score").GetComponent<Text>();
			_finalScore.text = "";
			_startBtn.onClick.AddListener(Bootloader.StartNew);
		}

		protected override void OnUpdate()
		{
			if(_player.Health.Length > 0)
			{
				// player alive
				_startBtn.gameObject.SetActive(false);
				_finalScore.gameObject.SetActive(false);
				_text.gameObject.SetActive(true);

				GameTimer gt = _timer.Timer[0];
				gt.Value = _timer.Timer[0].Value + Time.deltaTime;
				_timer.Timer[0] = gt;

				_txtBuffer = $"Timer: {gt.Value.ToString("F2")} / Health: {_player.Health[0].Value}";
				_text.text = _txtBuffer;
			}
			else
			{
				// player dead
				_startBtn.gameObject.SetActive(true);
				_text.gameObject.SetActive(false);

				_finalScore.gameObject.SetActive(true);

				if(_player.Health.Length == 0 && _lastPlayerAlive == true)
				{
					TweakStartBtn();
					int killCount = _timer.State[0].KillCount;
					_finalScore.text = $"Congrats!\nYou persisted {_timer.Timer[0].Value.ToString("F2")} secs\n{killCount} {(killCount > 1 ? "enemies" : "enemy")} killed";
				}
			}

			_lastPlayerAlive = _player.Health.Length > 0;
		}

		void TweakStartBtn()
		{
			RectTransform rt = _startBtn.GetComponent<RectTransform>();
			Vector3 btn_pos = rt.localPosition;
			btn_pos.y = string.IsNullOrEmpty(_txtBuffer) ? 0.0f : -130.0f;
			rt.localPosition = btn_pos;
		}
	}
}