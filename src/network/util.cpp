#include "util.h"

char * sockaddrToString(const struct sockaddr * sa)
{
    int maxlen = 255;
    char * string = (char *)calloc(maxlen, sizeof(char));

    switch(sa->sa_family)
    {
        case AF_INET:
            inet_ntop(AF_INET, &(((struct sockaddr_in *)sa)->sin_addr), string, maxlen);
            break;

        case AF_INET6:
            inet_ntop(AF_INET6, &(((struct sockaddr_in6 *)sa)->sin6_addr), string, maxlen);
            break;

        default:
            strncpy(string, "Unknown", maxlen);
            return NULL;
    }

    return string;
}