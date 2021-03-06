﻿namespace Infusion
{

    public class Item : GameObject
    {
        public Item(ObjectId id, ModelId type, ushort amount, Location3D location, Color? color, ObjectId? containerId, Layer? layer)
            : base(id, type, location)
        {
            Amount = amount;
            Color = color;
            ContainerId = containerId;
            Layer = layer;
        }

        private Item(ObjectId id, ModelId type, Location3D location) : base(id, type, location)
        {
        }

        public override bool IsOnGround => !Layer.HasValue && !ContainerId.HasValue;

        public virtual ushort Amount { get; private set; }

        public ObjectId? ContainerId { get; private set; }

        public Color? Color { get; private set; }

        public Layer? Layer { get; private set; }

        public override string ToString()
        {
            string canModifyNameText = CanRename ? " (modifiable)" : string.Empty;

            return
                $"Id: {Id}; Type: {Type}; Name: {Name}{canModifyNameText}; Amount: {Amount}; Location: {Location}; Color: {Color}; Container {ContainerId}; Layer: {Layer}";
        }

        protected override GameObject Duplicate()
        {
            var duplicate = new Item(Id, Type, Location);

            duplicate.CopyFrom(this);

            return duplicate;
        }

        protected virtual void CopyFrom(Item template)
        {
            Color = template.Color;
            Amount = template.Amount;
            ContainerId = template.ContainerId;
            Layer = template.Layer;
            Name = template.Name;
            CanRename = template.CanRename;
        }

        internal Item Update(ModelId type, ushort amount, Location3D location, Color? color, ObjectId? containerId,
            Layer? layer)
        {
            var updatedItem = Update(type, amount, location, color, containerId);
            updatedItem.Layer = layer;

            return updatedItem;
        }

        internal Item Update(ModelId type, ushort amount, Location3D location, Color? color, ObjectId? containerId)
        {
            var updatedItem = (Item)Duplicate();
            updatedItem.Location = location;
            updatedItem.Type = type;
            updatedItem.Amount = amount;
            updatedItem.Color = color;
            updatedItem.ContainerId = containerId;

            return updatedItem;
        }
    }
}