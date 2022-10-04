using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using Sirenix.OdinInspector;
using System;
using TheraBytes.BetterUi;
using UnityEngine.Assertions;
using Return.Framework.Grids;
using System.Linq;
using Return.UI;
using Return.Inputs;
using Return.Items;
using Random = UnityEngine.Random;

namespace Return.Inventory
{
    /// <summary>
    /// Inventory grid handler.
    /// </summary>
    public class InventoryUIHandler : BaseComponent // mWindowUI //,IPointerClickHandler
    {
        /// <summary>
        /// Inventory icon element container.
        /// </summary>
        [ShowInInspector]
        protected InventoryGrid Grid;

        #region Dev

        [Button]
        void Create(Vector2Int volume)
        {
            Grid = new InventoryGrid();
            Grid.Build(volume.x, volume.y);
        }

        [Button]
        void SearchSpace(Vector2Int start,Volume volume)
        {
            if(volume==default)
            {
                volume = new Volume(Random.Range(1, Grid.lines/2), Random.Range(1, Grid.rows/2));
            }

            if (Grid.SearchSpaceTopLeft(volume, out var indexes))
            {
                var slot = new InventoryUISlot() { Color = Random.ColorHSV() };

                //foreach (var index in indexes)
                //{
                //    Grid[index] = slot;
                //}

                Grid.GetMinMax(indexes, out var min, out var max);

                Debug.Log($"Min {min} Max {max}.");

                slot = new InventoryUISlot() { Color = Random.ColorHSV() };
                Grid[min] = slot;

                slot = new InventoryUISlot() { Color = Random.ColorHSV() };
                Grid[max] = slot;

                slot = new InventoryUISlot() { Color = Random.ColorHSV() };

                Grid.SetVolumeValue(min, max, slot);

            }
            else
                Debug.LogError($"Failure to find space for {volume}.");

        }

        #endregion


        #region Setup

        [SerializeField]
        InventoryEffects m_EffectPreset;
        public InventoryEffects EffectPreset { get => m_EffectPreset; set => m_EffectPreset = value; }


        /// <summary>
        /// Setup content dragging_volume.
        /// </summary>
        [Button]
        public virtual void Setup(int row, int line)
        {
            Assert.IsFalse(GridHandler == null);


            var prefabSlot = ContentSlot.GetComponent<RectTransform>();
            Assert.IsFalse(prefabSlot == null);
            var slotSize = prefabSlot.rect.width;

            // set ui layout
            {
                var uiSize = prefabSlot.rect.size.Multiply(row, line);

                if (GridHandler)
                {
                    GridLayout.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
                    GridLayout.constraintCount = row;
                    GridLayout.cellSize = new Vector2(slotSize, slotSize);
                    uiSize += GridHandler.offsetMax.Abs() + GridHandler.offsetMin.Abs();
                    //GridLayout.CalculateLayoutInputHorizontal();
                }

                //if (GridLayout)
                //    uiSize += new Vector2(GridLayout.padding.horizontal, GridLayout.padding.vertical);

                UIHanlder.sizeDelta = uiSize;
            }


            // set grid colume
            {
                Assert.IsFalse(ContentSlot == null);

                //  get require prefabSlot
                var slotNums = row * line;

                if (m_combineSlots)
                {
                    CombineSlot(row, line);

                }
                else
                {
                    bool createSlot = true;

                    //  calculate number of slots to add
                    if (slots == null)
                        slots = new(slotNums);
                    else if (slotNums > slots.Count)
                        slotNums -= slots.Count;
                    else
                        createSlot = false;

                    if (createSlot)
                    {
                        for (int i = 0; i < slotNums; i++)
                        {
                            var slot = Instantiate(ContentSlot, GridHandler);
                            //prefabSlot.hideFlags = HideFlags.HideAndDontSave;
                            slots.Add(slot);
                        }

                        foreach (var slot in slots)
                        {
                            slot.SetActive(true);
                        }
                    }
                    else
                    {
                        for (int i = 0; i < slots.Count; i++)
                        {
                            slots[i].SetActive(i < slotNums);
                        }
                    }
                }
            }



            if (Grid == null)
                Grid = new();

            Grid.worldPos = GetGridAnchor;//GridHandler.rect.position; //GridHandler.position;
            Grid.padding = Padding;

            //var slotSize = GridLayout.cellSize.width;// +GridLayout.spacing.Avg();
            //GridHandler.rect.width / rows;

            SetSlotSize(slotSize);

            Grid.Build(row, line);
        }

        #endregion

        #region IPointer

        public void OnPointerClick(PointerEventData e)
        {
            var pos = e.position;

            bool select = false;

            bool doubleClick = e.clickCount > 1;

            if (doubleClick)
            {
                Debug.Log("Double click.");
                return;
            }

            //if (doubleClick)
            //    e.clickCount = 0;

            if (e.button == PointerEventData.InputButton.Left)
                select = true;
            else if (e.button == PointerEventData.InputButton.Right)
            {
                if (DragHandle)
                {
                    DiscardDragging();
                }
                return;
            }
            else if(e.button == PointerEventData.InputButton.Middle)
            {
                dir = dir switch
                {
                    Grid2DDirection.Horizontal => Grid2DDirection.Vertical,
                    Grid2DDirection.Vertical => Grid2DDirection.Horizontal,
                    _ => throw new NotImplementedException(),
                };

                if(DragHandle && Grid.ValidIndex(last_pointer_index))
                {
                    var content = Grid[last_pointer_index];
                    if(content)
                    {
                        DragHandle.localRotation = GetRotation(content.IconData.Volume, dir);
                    }
     
                }

                return;
            }

            e.Use();

            Debug.Log($"On click : {pos}.");

            //if (DragHandle)
            //    rectPos += drag_pointer_offset.XY();

            // convert screen space pointer to ui space position
            var valid = GetRectPosition(pos,out var rect_pos);

            Assert.IsTrue(valid, $"Pointer space invalid {rect_pos}");

            if (select)
                ProccessLeftClick(rect_pos);
        }

        /// <summary>
        /// Convert screen position to local rect position.
        /// </summary>
        public virtual bool GetRectPosition(Vector2 pos,out Vector2 rect_pos)
        {
            return RectTransformUtility.ScreenPointToLocalPointInRectangle(GridHandler, pos, null, out rect_pos);
        }
 
        #endregion

        #region User Control

        [ShowInInspector]
        static Grid2DDirection dir = Grid2DDirection.Horizontal;

        #region Drag Drop Switch

        /// <summary>
        /// Cache inventory instance to proccess cross ui interaction.
        /// </summary>
        static InventoryUIHandler lastHandler;

        /// <summary>
        /// Cache last interact grid offset_index.
        /// </summary>
        static Vector2Int last_pointer_index;

        /// <summary>
        /// Cache last volume offset.
        /// </summary>
        [Obsolete]
        static Vector2Int last_pointer_Index_offset;


        /// <summary>
        /// Cache offset position between pointer and dragging icon.
        /// </summary>
        [Obsolete]
        static Vector3 drag_pointer_offset;

        /// <summary>
        /// Cache coordinate to apply icon position revert.
        /// </summary>
        static PR lastIconCoordinate;

        static RectTransform ps_DragContent;

        protected virtual void SetDraggingIcon(RectTransform rt,Vector2Int index=default)
        {
            ps_DragContent = rt;

            if (rt)
            {
                drag_pointer_offset = default;

                return;

                Grid.GetVolume(index,out var offset,out var volume);
 
                drag_pointer_offset = GetIconOffset(rt, offset,volume);

       

                return;

                // get offset_index offset and convert to position
                var pointer = Input.mousePosition;

                RectTransformUtility.ScreenPointToLocalPointInRectangle(rt, pointer, null, out var rect_pos);
                Grid.GetIndex(rect_pos, out var pointer_index);
                Debug.Log($"Start dragging with content offset_index {pointer_index}.");
                Grid.GetIndexPosition(pointer_index, out var pointer_pos);

                //pointer_pos = value.TransformPoint(pointer_pos);

                //drag_pointer_offset = value.position.XY() - pointer_pos;

                drag_pointer_offset = pointer_pos;

                Debug.Log($"Dragging offset : {drag_pointer_offset} position : {pointer_pos} pointer {pointer}.");
            }
        }

        /// <summary>
        /// Dragging UI transform.
        /// </summary>
        RectTransform DragHandle
        {
            get => ps_DragContent;
        }

        /// <summary>
        /// Get icon offset for pointer.
        /// </summary>
        public Vector2 GetIconOffset(RectTransform rt,Vector2Int offset_index,Volume volume)
        {
            var size = rt.rect.size;

            if (offset_index.x > 0)
                offset_index.x--;

            if (offset_index.y > 0)
                offset_index.y--;

            var ratio = new Vector2((offset_index.x + 0.5f) / volume.width, -(offset_index.x + 0.5f) / volume.height);

            size = size.Multiply(ratio);

            Debug.Log(
           $"Set drag icon offset : {size}\n" +
           $"ratio {ratio}\n" +
           $"offset {offset_index}\n" +
           $"volume {volume}");

            return size;
        }


        public virtual void DiscardDragging()
        {
            if (DragHandle == null)
                return;

            // revert dragging icon coordinate
            DragHandle.SetLocalPR(lastIconCoordinate);

            // clean dragging cache
            SetDraggingIcon(null);
            lastHandler = null;
            lastIconCoordinate = default;
            drag_pointer_offset = default;
        }

        public virtual bool GetIconIndexes(InventoryUISlot lastContent,out List<Vector2Int> castIndexes)
        {
            bool validDrop = true;

            var volume = lastContent.IconData.Volume;

            var castPoints = lastContent.GetWorldPositions();
            castIndexes = new(volume.capacity);

            foreach (var point in castPoints)
            {
                if (!GetRectPosition(point, out var rectPos))
                {
                    Debug.LogError($"Failure to get rect position with {point}.");
                    validDrop = false;
                    break;
                }

                rectPos -= GridHandler.rect.position;

                if (!Grid.GetIndex(rectPos, out var index))
                {
                    Debug.LogError($"Failure to get grid index with {rectPos}.");
                    validDrop = false;
                    break;
                }

                var content = Grid[index];

                if (content == null || content == lastContent)
                {
                    castIndexes.Add(index);
                    continue;
                }


                // valid switch
                Debug.Log($"Switch content to [{index}] failure.");
                validDrop = false;

                break;
            }

            return validDrop;
        }

        public virtual void ProccessLeftClick(Vector2 pointer_pos)
        {
            var rect = GridHandler.rect;

            Debug.Log($"rect : {rect} pointer_pos : {pointer_pos}");

            // valid pointer inside UI rect
            if (!rect.Contains(pointer_pos))
            {
                Debug.Log($"Click point is out of content rect.");

                // valid selection
                if (lastHandler == null)
                    return;

                //  drop content
                Debug.LogError("Drop content out side the bounds.");
                return;
            }
            else
            {
                // convert pointer position to non-dragging_offset grid space
                pointer_pos -= rect.position;

                // convert pointer space(left-bottom) to UI space(left-top) and rect dragging_offset
                //pointer_pos.y = rect.height - pointer_pos.y;// + GridLayout.padding.top;
                //pointer_pos.lines += GridLayout.padding.left;
            }

            // get pointer offset_index
            if (!Grid.GetIndex(pointer_pos, out var pointer_index))
            {
                Debug.LogError("Pointer not inside grid.");
                return;
            }

            if (DragHandle == null)
                BeginDrag(pointer_index);
            else
                DoDrop(pointer_index);
        }

        protected virtual void BeginDrag(Vector2Int pointer_index)
        {
            var content = Grid.GetValue(pointer_index);

            Debug.Log($"On drag [{pointer_index.x},{pointer_index.y}] targeted : {content}.");


            // Dev create new content
            if (content == null)
            {
                var voluleIndexes = Item.GetVolumeIndexes(pointer_index, dir);

                if (Grid.ValidSlots(voluleIndexes))
                {
                    content = CreateSlotBinding(Item);
                    SetContent(content, voluleIndexes);
                    Debug.Log($"Create content: {pointer_index}");
                    return;
                }
            }

            if (content != null)
            {
                var valid = Grid.GetVolume(pointer_index, out var offset, out var volume);
                Assert.IsTrue(valid, "Non valid content found in grid");

                // sign inventory
                lastHandler = this;
                // key pointer offset_index
                last_pointer_index = pointer_index;
                // key pointer dragging_offset
                last_pointer_Index_offset = offset;
                // cache icon direction
                dir = content.IconData.GetDirection(volume);

                // attach image to pointer
                SetDraggingIcon(content.IconHandler, pointer_index);
                // cache dragging content coordinate
                lastIconCoordinate = content.IconHandler.GetLocalPR();

                Debug.Log($"OnDrag {DragHandle}.");
            }
        }

        protected virtual void DoDrop(Vector2Int pointer_index)
        {
            // check draging content exist 
            var lastContent=Grid.GetValue(last_pointer_index);

            bool validDrop = lastContent != null;

            // valid draging content
            // ** cross inventory
            // **run out **destroy
            if (!validDrop)
            {
                Debug.LogError($"Last selected content has been dispose : {lastContent?.IconData}.");
                DiscardDragging();
                return;
            }


            #region cast content volume from UI indexes
            {
                var volume = lastContent.IconData.Volume;

                validDrop = GetIconIndexes(lastContent,out var castIndexes);

                foreach (var castIndex in castIndexes)
                    Debug.Log($"Added index [{castIndex.x},{castIndex.y}].");

                if (validDrop)
                {
                    // remove last dragging_volume cache
                    if (Grid.GetContentIndexes(last_pointer_index, out var removeList))
                    {
                        foreach (var removeIndex in removeList)
                        {
                            Debug.Log($"Remove remove {removeIndex}.");
                            Grid.SetValue(removeIndex, null);
                            //Debug.Log(Grid[removeIndex] == null);
                        }
                    }

                    // binding content
                    SetContent(lastContent, castIndexes);
                    SetDraggingIcon(null);
                    return;
                }
                else
                    Debug.LogError($"Pointer position {pointer_index} is invalid to place dragging content {volume}.");

                DiscardDragging();
                return;
            }
            #endregion

            validDrop = Grid.GetVolume(last_pointer_index, out var lastOffset, out var lastVolume);


            #region cast index from grid volume
            // cast dragging volume to pointer
            //{
            //    Assert.IsTrue(validDrop);

            //    InventoryUISlot switchable = null;

            //    foreach (var index in lastVolume)
            //    {
            //        var slotIndex = pointer_index + lastOffset + index;
            //        // out of border
            //        if (!Grid.ValidIndex(slotIndex))
            //        {
            //            validDrop = false;
            //            break;
            //        }

            //        var slot = Grid[slotIndex];
            //        // can not place dragging content
            //        if (slot != null && slot != lastContent)
            //        {
            //            if (switchable == null)
            //            {
            //                switchable = slot;
            //                continue;
            //            }

            //            validDrop = false;
            //            break;
            //        }
            //    }

            //    // unable to drop content
            //    if (!validDrop)
            //    {
            //        // valid switch content
            //        if (switchable != null)
            //        {
            //            // switch content

            //            // single
            //            {
            //                // capacity and points
            //                if (switchable.IconData.Volume == lastVolume)
            //                {
            //                    // valid origin match volume

            //                    //{
            //                    //    var lastIndexes = lastVolume.GetIndexes(last_pointer_index, last_pointer_Index_offset);

            //                    //    // switch content
            //                    //    foreach (var index in lastIndexes)
            //                    //    {
            //                    //        Grid.SetValue(index, switchable);
            //                    //    }
            //                    //}



            //                }
            //            }

            //            // multi
            //            {
            //                //var points

            //                //if(Grid.ValidPack())
            //            }

            //        }

            //        DiscardDragging();
            //        return;
            //    }

            //    {
            //        var content = Grid.GetValue(pointer_index);
            //        Debug.Log($"On drop [{pointer_index.x + 1},{pointer_index.y + 1}] targeted : {content}.");
            //    }
            //}
            #endregion


            #region cast content volume from volume data
            try
            {
                // get icon direction from dragging content  
                //var dir = lastContent.IconData.GetDirection(lastVolume);

                // get drop volume indexes
                var newIndexes = lastContent.IconData.Volume.GetIndexes(pointer_index,lastOffset, dir);
                //var newIndexes = lastContent.IconData.GetVolumeIndexes(pointer_index + lastOffset, dir);

                // valid new volume
                if (Grid.ValidSlots(newIndexes))
                {
                    // removeIndex last dragging_volume cache
                    if (Grid.GetContentIndexes(last_pointer_index, out var removeList))
                    {
                        foreach (var remove in removeList)
                        {
                            var slotIndex = remove;//last_pointer_index + dragging_offset + removeIndex;
                            Debug.Log($"Remove remove {remove}.");
                            Grid.SetValue(remove, null);
                            Debug.Log(Grid[remove] == null);
                        }
                    }
                    else
                        Debug.LogError("Non exist content slot found.");

                    // binding content
                    SetContent(lastContent, newIndexes);
                }
                else
                {
                    Debug.LogError($"Pointer position {pointer_index} is invalid to place dragging content {lastVolume}.");
                    DiscardDragging();
                    return;
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
            finally
            {
                lastHandler = null;
                SetDraggingIcon(null);
            }

            #endregion
        }



        protected virtual void Update()
        {
            if (DragHandle != null)
                DragHandle.position = Input.mousePosition + drag_pointer_offset;

            if (DragHandle && false)
            {
                Debug.Log("Handle position " + DragHandle.position);

                GetRectPosition(DragHandle.position, out var rectPos);

                rectPos -= GridHandler.rect.position;

                if (!Grid.GetIndex(rectPos, out var index))
                    return;

                var lastContent = Grid.GetValue(last_pointer_index);

                if (lastContent == null)
                    return;

                var validDrop = Grid.GetVolume(last_pointer_index, out var offset, out var lastVolume);

                var castPoints = lastContent.GetWorldPositions();

                foreach (var point in castPoints)
                {
                    if (!GetRectPosition(point, out rectPos))
                    {
                        Debug.LogError($"Failure to get rect position with {point}.");
                        validDrop = false;
                        break;
                    }

                    rectPos -= GridHandler.rect.position;

                    if (!Grid.GetIndex(rectPos, out index))
                    {
                        Debug.LogError($"Failure to get grid index with {rectPos}.");
                        validDrop = false;
                        break;
                    }

                    var content = Grid[index];

                    if (content == null || content == lastContent)
                    {
                        Debug.Log($"Cast index [{index.x},{index.y}].");
                        continue;
                    }
                }
            }

        }



        #endregion



        #endregion

        #region Dev

        [SerializeField]
        Vector2Int SlotNums;

        [SerializeField]
        InveonoryIcon Item;

        private void Start()
        {
            if (UIHanlder == null)
                UIHanlder = InstanceIfNull<RectTransform>();

            // hide prefab
            if(ContentSlot)
                ContentSlot.SetActive(false);

            if(ContentIcon)
                ContentIcon.gameObject.SetActive(false);

            // set prefabSlot
            //Setup(SlotNums.x, SlotNums.y);

            // add content
            //var indexes = Item.GetVolumeIndexes(Vector2Int.zero, dir);

            //if (Grid.ValidSlots(indexes))
            //{
            //    var content = CreateSlotBinding(Item);
            //    SetContent(content,indexes);
            //}

            
        }


        [SerializeField]
        bool m_combineSlots;

        [Button]
        void CombineSlot(int row, int line)
        {
            if (!ContentSlot.TryGetComponent<Image>(out var img))
                throw new InvalidOperationException($"Missing prefabSlot image component");

            var texture = img.mainTexture;

            var combineImg = new Texture2D(texture.width * row, texture.height * line);

            for (int x = 0; x < row; x++)
            {
                for (int y = 0; y < line; y++)
                {
                    //combineImg.SetPixels(width,height)
                }
            }
        }

        #endregion

        #region IInventoryGUI

        protected Dictionary<int, InventoryUISlot> cacheUI = new();

        public virtual void UpdateContents(IEnumerable<object> objects)
        {
            var dic = objects.ToDictionary(x => x.GetHashCode(), x => x);

            if (cacheUI == null)
            {
                cacheUI = new(objects.Count());
            }
            else
            {
                var old = cacheUI.Keys.CacheLoop();

                foreach (int hash in old)
                {
                    if (!dic.ContainsKey(hash))
                        RemoveContent(hash);
                }
            }

            foreach (var pair in dic)
            {
                if (!cacheUI.ContainsKey(pair.Key))
                {
                    PushContent(pair.Value);
                }
            }
        }

        public virtual void PushContent(object obj)
        {
            Debug.Log($"Push inventory conent : {obj}.");

            if(obj is IArchiveContent archive)
            {
                if(archive.content is IPickup item)
                {
                    var icon = item.Preset.Icon;

                    if(Grid.SearchSpaceTopLeft(icon.Volume,out var indexes))
                    {
                        var content = CreateSlotBinding(icon);
                        SetContent(content, indexes);
                    }
                    else
                        Debug.LogException(new NotImplementedException("Inventory ui has not enough space to inseert icon."));

                }
                else
                    Debug.LogException(new NotImplementedException($"Inventory ui unknow type not finish : {archive}."));

            }
            else
            {
                // add content
                var icon = new InveonoryIcon() { Volume = Vector2Int.one };

                if (Grid.SearchSpaceTopLeft(icon.Volume, out var indexes))
                {
                    var content = CreateSlotBinding(icon);
                    SetContent(content, indexes);
                }
                else
                    Debug.LogException(new NotImplementedException("Inventory ui has not enough space to inseert icon."));
            }
        }

        public virtual void RemoveContent(int hash)
        {
            if (cacheUI.TryGetValue(hash, out var ui))
            {
                var valid = GetIconIndexes(ui, out var castIndexes);
                Assert.IsTrue(valid);
                SetContent(null, castIndexes);

                ui.Dispose();
                ui.IconHandler.gameObject.Destroy();
                cacheUI.Remove(hash);
            }
            else
                Debug.LogException(new KeyNotFoundException($"Remove target is not inside ui cache."));
        }

        public virtual void RemoveContent(object obj)
        {
            RemoveContent(obj.GetHashCode());
        }

        #endregion

        #region Inventory Slots

        [SerializeField]
        RectTransform m_GridHandler;

        [Tooltip("Config grid spacing.")]
        [SerializeField]
        GridLayoutGroup GridLayout;

        [Tooltip("Prefab to instantiate prefabSlot")]
        [SerializeField]
        GameObject m_ContentSlot;

        /// <summary>
        /// Transform to add prefabSlot ui element.
        /// </summary>
        public RectTransform GridHandler { get => m_GridHandler; set => m_GridHandler = value; }

        /// <summary>
        /// Preset prefabSlot element to build the grid.
        /// </summary>
        public GameObject ContentSlot { get => m_ContentSlot; set => m_ContentSlot = value; }



        /// <summary>
        /// Cache prefabSlot element
        /// </summary>
        [NonSerialized]
        protected List<GameObject> slots;

        #endregion



        #region Inveontory UI

        [SerializeField]
        RectTransform m_UIHandler;

        /// <summary>
        /// Transform to set UI rect width and height.
        /// </summary>
        public RectTransform UIHanlder { get => m_UIHandler; set => m_UIHandler = value; }

        #endregion

        #region Content Icon

        [SerializeField]
        RectTransform m_IconHandler;

        [SerializeField]
        Image m_ContentIcon;

        /// <summary>
        /// Transform to hold content icon.
        /// </summary>
        public RectTransform IconHandler { get => m_IconHandler; set => m_IconHandler = value; }

        /// <summary>
        /// Sprite preset to instantiate content icon.
        /// </summary>
        public Image ContentIcon { get => m_ContentIcon; set => m_ContentIcon = value; }


        #region Layout

        public virtual void SetSlotSize(float cellSize)
        {
            // update image slotSize

            // update grid logic
            Grid.cellSize = cellSize;
        }


        protected virtual RectOffset Padding()
        {
            if (GridLayout)
                return GridLayout.padding;
            else
                return null;
        }

        protected virtual Vector2 GetGridAnchor()
        {
            //if (GridHandler)
            //    return new Vector2(-GridHandler.rect.lines,-GridHandler.rect.rows);
            ////return GridHandler.anchoredPosition;
            //else
            return Vector2.zero;
        }

        public static int GetAngle(Volume volume, Grid2DDirection dir)
        {
            // square
            if (volume.width == volume.height)
                return 0;

            return dir switch
            {
                Grid2DDirection.Horizontal => 0,
                Grid2DDirection.Vertical => 90,
                _ => throw new KeyNotFoundException($"{dir} is invalid direction in inventory."),
            };
        }

        public static Quaternion GetRotation(Volume volume, Grid2DDirection dir)
        {
            return Quaternion.Euler(0, 0, GetAngle(volume, dir));
        }

        #endregion


        protected virtual InventoryUISlot CreateSlotBinding(InveonoryIcon icon)
        {
            var slot = new InventoryUISlot()
            {
                IconData = icon,
            };

            var img = GetIcon(icon.Volume);

            if (img is BetterImage betterImg)
                betterImg.sprite = icon.Sprite;
            else
                img.sprite = icon.Sprite;


            slot.Icon = img;
            slot.IconHandler = img.rectTransform;

            return slot;
        }

        protected virtual Image GetIcon(Volume size)
        {
            var img = Instantiate(ContentIcon, IconHandler);
            img.gameObject.SetActive(true);
            var cellSize=Grid.cellSize;
            img.rectTransform.sizeDelta = new Vector2(size.width,size.height).Multiply(cellSize);

            return img;
        }


        /// <summary>
        /// Binding content to grid and config the ui
        /// </summary>
        /// <param name="slot">Sprite data.</param>
        /// <param name="indexes">Volume indexes to align.</param>
        public virtual void SetContent(InventoryUISlot slot, IEnumerable<Vector2Int> indexes)
        {
            Debug.Log($"Set content with {slot.IconHandler}.");

            foreach (var contentIndex in indexes)
            {
                Grid.SetValue(contentIndex.x, contentIndex.y, slot);
            }

            if(slot)
                SetContentUI(slot, indexes);
        }

        /// <summary>
        /// Set icon position to grid.
        /// </summary>
        /// <param name="slot">Sprite data.</param>
        /// <param name="indexes">Volume indexes to align.</param>
        public virtual void SetContentUI(InventoryUISlot slot, IEnumerable<Vector2Int> indexes)
        {
            Debug.Log($"Set content UI with {slot.IconHandler}.");

            var pos = Vector2.zero;

            foreach (var contentIndex in indexes)
            {
                Grid.GetIndexPosition(contentIndex, out var middle);
                pos += middle;
            }

            var icon = slot.IconData;

            slot.IconHandler.localPosition = pos.Multiply(1f / icon.Volume.capacity);
            slot.IconHandler.localRotation = GetRotation(icon.Volume, dir);
        }

        #endregion

    }

}