using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using XLua;

namespace Brag_Sieve_Drag
{
    [LuaCallCSharp]
    public class DragScript : MonoBehaviour, IBeginDragHandler, IDragHandler, IDropHandler, IDisposable
    {

        //private Action<bool> BeginAction = null;
        //private Action<bool> DragAction = null;
        //private Action<bool> DropAction = null;
       
        [CSharpCallLua]
        public delegate bool DragAction();
       
        private DragAction OnDragAction = null;
       
        private Action DoSth;

        private Vector3 position;
        private float x;
        private float z;
    

        public void Dispose()
        {
    
            OnDragAction = null;
      
        }

        public void Init(DragAction funcDrag /*ref bool open*/, Action doSth)
        {
            OnDragAction = funcDrag;
            this.DoSth = doSth;
            position = transform.position;
            x = transform.position.x;
            z = transform.position.z;
            //isCanOpen = open;

        }

        public void OnBeginDrag(PointerEventData eventData)
        {
            if (!OnDragAction())
            {
                return;
            }
        }

        public void OnDrag(PointerEventData eventData)
        {
            //throw new System.NotImplementedException();
            
            if (!OnDragAction())
            {
                return;
            }
            if (position.y > Input.mousePosition.y)
            {                
                return;
            }
            transform.position = new Vector3(x, Input.mousePosition.y, z);
            
            if (transform.position.y - position.y >= 100)
            {
                gameObject.SetActive(false);
                DoSth();                                  
                transform.position = position;
            }
        }

        public void OnDrop(PointerEventData eventData)
        {
            //throw new System.NotImplementedException();   
            transform.position = position;
        }
    }
}

