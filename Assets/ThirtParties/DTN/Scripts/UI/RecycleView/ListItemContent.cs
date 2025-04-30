using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Cooldhands.UI
{
	public class ListItemContent : MonoBehaviour, ISelectHandler, IPointerEnterHandler, IPointerExitHandler, IPointerClickHandler
	{
		public enum eState {
			Ready,
			Loading,
			LoadCompleted,
		}
		
		[SerializeField] private int _index = 0;
		[SerializeField] private eState _state = eState.Ready;
		[SerializeField] private bool _isHide = false;
		private RectTransform _rectTransform = null;
		private RecycleGridLayoutGroup _recycleGridLayout = null;
		private bool _dragging = false;
		private object _data = null;

		public int index {
			get { return _index; }
			set { _index = value; }
		}
		public eState state {
			get { return _state; }
			set { 
				if( _isHide == true && value == eState.LoadCompleted ) {
					_state = eState.Ready;
				} else {
					_state = value;
				}
			}
		}
		public bool isHide {
			get { return _isHide; }
			set {
				_isHide = value;
				if( _isHide == true && _state == eState.LoadCompleted ) {
					_state = eState.Ready;
				}
			}
		}
		public RectTransform rectTransform {
			get {
				if( _rectTransform == null ) _rectTransform = this.transform as RectTransform;
				return _rectTransform;
			}
			set { _rectTransform = value; }
		}
		public RecycleGridLayoutGroup recycleGridLayout  {
			get {
				return _recycleGridLayout;
			}
			set { _recycleGridLayout = value; }
		}

		public object Data 
		{
			get { return _data; }
			set { _data = value; }
		}

		/// <summary>
		/// Callback to scroll only if the object is selected.
		/// </summary>
		public void OnSelect(BaseEventData eventData)
		{		
			if(_recycleGridLayout != null && !_dragging)
			{
				_recycleGridLayout.ScrollToPosition(_index);
			}
		}

		public void OnPointerEnter(PointerEventData eventData)
		{
			_dragging = true;
		}

		public void OnPointerExit(PointerEventData eventData)
		{
			_dragging = false;
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if(_recycleGridLayout != null)
			{
				_recycleGridLayout.ScrollToPosition(_index);
			}
		}
	}
}