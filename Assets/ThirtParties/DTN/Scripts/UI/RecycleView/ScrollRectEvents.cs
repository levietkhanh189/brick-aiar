using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Cooldhands.UI
{
    public class ScrollRectEvents : MonoBehaviour, IEndDragHandler, IBeginDragHandler, IScrollHandler
    {
        #region Variables
        [SerializeField] private RecycleGridLayoutGroup _recycleGridLayout = null;
        [SerializeField] private ScrollRect _scrollRect;
        bool _swipeLeft = false;
        bool _swipeUp = false;
        bool _isDragging = false;
        bool _scrollChanged = false;

        #endregion

        #region Properties

        public RecycleGridLayoutGroup RecycleGridLayout
        {
            get{ return _recycleGridLayout; }
            set { _recycleGridLayout = value; }
        }

        public ScrollRect ScrollRect
        {
            get{ return _scrollRect; }
            set { _scrollRect = value; }
        }

        #endregion

        /// <summary>
        /// Enable snap
        /// </summary>
        public void EnableSnap()
        {
            this.enabled = true;
            /*if( _scrollRect != null ) {
                _scrollRect.onValueChanged.AddListener(OnScrollEvent);
            }*/
        }

        /// <summary>
        /// Disable snap
        /// </summary>
        public void DisableSnap()
        {
            this.enabled = false;
            /*if( _scrollRect != null ) {
                _scrollRect.onValueChanged.RemoveListener(OnScrollEvent);
            }*/
        }

        public void OnEndDrag(PointerEventData eventData)
        {
            Vector3 dragVectorDirection = (eventData.position - eventData.pressPosition).normalized;
            
            _swipeLeft = dragVectorDirection.x < 0 ? false : true;
            _swipeUp = dragVectorDirection.y > 0 ? false : true;
            _isDragging = false;
            _scrollChanged = true;
        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            _isDragging = true;
        }

        public void OnScroll(PointerEventData eventData)
        {               
            MoveScroll(eventData.scrollDelta);
        }

        private void MoveScroll(Vector2 scrollDelta)
        {
            if(_recycleGridLayout.vertical && !IsLimitScrollY())
            {
                if(scrollDelta.y > 0)
                {
                    _recycleGridLayout.ScrollToPrev();
                }
                else if(scrollDelta.y < 0){
                    _recycleGridLayout.ScrollToNext();
                }
            }
            if(_recycleGridLayout.horizontal && !IsLimitScrollX()){
                if(scrollDelta.x > 0 || scrollDelta.y < 0)
                {
                    _recycleGridLayout.ScrollToPrev();
                }
                if(scrollDelta.x < 0 || scrollDelta.y > 0){
                    _recycleGridLayout.ScrollToNext();    
                }
            }
        }

        private void OnScrollEvent(Vector2 scrollPos)
        {
            //_scrollChanged = true;
        }

        /// <summary>
        /// Update is called every frame, if the MonoBehaviour is enabled.
        /// </summary>
        void LateUpdate()
        {
            if((!_isDragging && _scrollChanged && !_recycleGridLayout.isAutoScrolling))
            {
                float minVel = ((_recycleGridLayout.vertical ? _recycleGridLayout.cellSize.y : _recycleGridLayout.cellSize.x) * 1000f)/100f;
                if(_scrollRect.velocity == Vector2.zero)
                {
                    minVel = 0;
                }
                VerifyScroll(_scrollRect.normalizedPosition, minVel);
            }
        }
        private void VerifyScroll(Vector2 scrollPos, float minVel)
        {
            if(_recycleGridLayout.vertical)
            {
                if (Mathf.Abs(_scrollRect.velocity.y) <= minVel && Mathf.Abs(_scrollRect.velocity.y) >= 0f 
                && !IsLimitScrollY()) {
                    _recycleGridLayout.OnEndDrag(_swipeLeft, _swipeUp);
                    _scrollChanged= false;
                }
            }
            else
            {
                if (Mathf.Abs(_scrollRect.velocity.x) <= minVel && Mathf.Abs(_scrollRect.velocity.x) >= 0f
                && !IsLimitScrollX()) {
                    _recycleGridLayout.OnEndDrag(_swipeLeft, _swipeUp);
                    _scrollChanged= false;
                }
            }
        }
        private bool IsLimitScrollX()
        {
            return !(_scrollRect.normalizedPosition.x > 0 
            && _scrollRect.normalizedPosition.x < 1 
            && !Mathf.Approximately(_scrollRect.normalizedPosition.x, 1) 
            && !Mathf.Approximately(_scrollRect.normalizedPosition.x, 0));
        }
        private bool IsLimitScrollY()
        {
            return !(_scrollRect.normalizedPosition.y > 0f 
            && _scrollRect.normalizedPosition.y < 1f 
            && !Mathf.Approximately(_scrollRect.normalizedPosition.y, 1) 
            && !Mathf.Approximately(_scrollRect.normalizedPosition.y, 0));
        }
    }
}