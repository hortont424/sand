#include "remotePeer.h"

RemotePeer::RemotePeer(int sock)
{
    this->sock = sock;
}

void RemotePeer::UpdateName(const char * name)
{
    LOG(INFO) << "RemotePeer::UpdateName() " << name;

    this->name = name;
}

const char * RemotePeer::GetName()
{
    return name;
}