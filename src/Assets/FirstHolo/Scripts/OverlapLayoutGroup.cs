//    Copyright 2016 United States Government as represented by the
//    Administrator of the National Aeronautics and Space Administration.
//    All Rights Reserved.
//
//    Licensed under the Apache License, Version 2.0 (the "License");
//    you may not use this file except in compliance with the License.
//    You may obtain a copy of the License at
//      http://www.apache.org/licenses/LICENSE-2.0
//
//    Unless required by applicable law or agreed to in writing, software
//    distributed under the License is distributed on an "AS IS" BASIS,
//    WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
//    See the License for the specific language governing permissions and
//    limitations under the License.



using UnityEngine;
using System.Collections;

namespace UnityEngine.UI
{
    public class OverlapLayoutGroup : LayoutGroup
    {
        protected OverlapLayoutGroup()
        {
        }

        public override void CalculateLayoutInputHorizontal()
        {
            base.CalculateLayoutInputHorizontal(); // No Vertical equivalent, it just lists the children to include.
            CalculateLayoutInputForAxis(0);
        }

        public override void CalculateLayoutInputVertical()
        {
            CalculateLayoutInputForAxis(1);
        }

        void CalculateLayoutInputForAxis(int axis)
        {
            // We need to reserve space for the padding.
            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);
            float totalmin = combinedPadding;
            float totalpreferred = combinedPadding;
            // And for the largest child.
            float min = 0;
            float preferred = 0;
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                min = Mathf.Max(min, LayoutUtility.GetMinSize(child, axis));
                preferred = Mathf.Max(preferred, LayoutUtility.GetPreferredSize(child, axis));
            }
            totalmin += min;
            totalpreferred += preferred;
            // We ignore flexible size for now, I have not decided what to do with it yet.
            SetLayoutInputForAxis(totalmin, totalpreferred, -1, axis);
        }

        public override void SetLayoutHorizontal()
        {
            SetLayoutAlongAxis(0);
        }

        public override void SetLayoutVertical()
        {
            SetLayoutAlongAxis(1);
        }

        void SetLayoutAlongAxis(int axis)
        {
            // Take all the space, except the padding.
            float combinedPadding = (axis == 0 ? padding.horizontal : padding.vertical);
            float size = rectTransform.rect.size[axis] - combinedPadding;
            // Everybody starts at the same place.
            float pos = GetStartOffset(axis, 0);
            // Overlap all the things.
            for (int i = 0; i < rectChildren.Count; i++)
            {
                RectTransform child = rectChildren[i];
                SetChildAlongAxis(child, axis, pos, size);
            }
        }
    }
}
