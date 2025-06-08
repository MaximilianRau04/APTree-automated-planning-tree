using System.Collections;

public interface IBTFlowNode : IBTNode , IEnumerable
{
    // only this node allows for adding children
    IBTNode AddChild(IBTNode Inode);

}