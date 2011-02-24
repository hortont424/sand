/*
 * Copyright 2011 Tim Horton and Pete Schirmer. All rights reserved.
 *
 * Redistribution and use in source and binary forms, with or without modification,
 * are permitted provided that the following conditions are met:
 *
 * 1. Redistributions of source code must retain the above copyright notice,
 * this list of conditions and the following disclaimer.
 *
 * 2. Redistributions in binary form must reproduce the above copyright notice,
 * this list of conditions and the following disclaimer in the documentation
 * and/or other materials provided with the distribution.
 *
 * THIS SOFTWARE IS PROVIDED BY THE CONTRIBUTORS "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL ANY AUTHORS OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "mdns.h"

#ifdef DARWIN

#include <CoreServices/CoreServices.h>
#include <glog/logging.h>

typedef struct
{
    MDNSBrowseCallback add, remove;
    void * info;
} MDNSResponderCallbacks;

void MDNSResponderRegistrationCallback(CFNetServiceRef svc, CFStreamError * err, void * info)
{
    if(err->error == 0)
    {
        LOG(INFO) << "Successfully registered mDNS service";
    }
    else
    {
        LOG(ERROR) << "Failed to register mDNS service";
    }
}

void MDNSResponderBrowseCallback(CFNetServiceBrowserRef browser, CFOptionFlags flags,
                                 CFTypeRef domainOrService, CFStreamError * error, void * info)
{
    CFNetServiceRef svc = (CFNetServiceRef)domainOrService;
    MDNSResponderCallbacks * cbs = (MDNSResponderCallbacks *)info;
    MDNSService * mdnsService = new MDNSService();
    CFDataRef saddrData;
    sockaddr saddr;

    CFNetServiceResolveWithTimeout(svc, 3, NULL);

    mdnsService->name = CFStringGetCStringPtr(CFNetServiceGetName(svc), CFStringGetSystemEncoding());
    mdnsService->type = CFStringGetCStringPtr(CFNetServiceGetType(svc), CFStringGetSystemEncoding());
    mdnsService->port = CFNetServiceGetPortNumber(svc);

    // TODO: multiple addresses!

    saddrData = (CFDataRef)CFArrayGetValueAtIndex(CFNetServiceGetAddressing(svc), 0);
    CFDataGetBytes(saddrData, CFRangeMake(0, CFDataGetLength(saddrData)), (UInt8*)&saddr);
    mdnsService->address = saddr;

    if(flags & kCFNetServiceFlagRemove)
    {
        if(cbs->remove)
            cbs->remove(mdnsService, cbs->info);
    }
    else
    {
        if(cbs->add)
            cbs->add(mdnsService, cbs->info);
    }
}

#pragma mark Public Functions

void MDNSResponderTick()
{
    CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0, true);
}

void MDNSRegister(uint16_t port)
{
    CFNetServiceRef svc;
    CFNetServiceClientContext * clientCtx;
    CFStringRef domain, serviceType, name;

    domain = CFSTR("local");
    serviceType = CFSTR("_sand._tcp");
    name = CFSTR("Sand");

    LOG(INFO) << "Registering mDNS service on port " << port;

    svc = CFNetServiceCreate(NULL, domain, serviceType, name, port);

    if(!svc)
    {
        LOG(ERROR) << "CFNetServiceCreate() failed!";

        return;
    }

    clientCtx = (CFNetServiceClientContext *)calloc(1, sizeof(CFNetServiceClientContext));

    if(!CFNetServiceSetClient(svc, MDNSResponderRegistrationCallback, clientCtx))
    {
        LOG(ERROR) << "CFNetServiceSetClient() failed!";

        free(clientCtx);

        return;
    }

    CFNetServiceScheduleWithRunLoop(svc, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);

    if(!CFNetServiceRegisterWithOptions(svc, 0, NULL))
    {
        LOG(ERROR) << "CFNetServiceRegisterWithOptions() failed!";

        CFNetServiceSetClient(svc, NULL, NULL);

        free(clientCtx);

        return;
    }
}

void MDNSBrowse(MDNSBrowseCallback addCb, MDNSBrowseCallback removeCb, void * info)
{
    CFNetServiceBrowserRef browser;
    CFNetServiceClientContext * clientCtx;
    CFStringRef domain, serviceType, name;
    MDNSResponderCallbacks * cbs;

    domain = CFSTR("local");
    serviceType = CFSTR("_sand._tcp");

    clientCtx = (CFNetServiceClientContext *)calloc(1, sizeof(CFNetServiceClientContext));
    cbs = (MDNSResponderCallbacks *)calloc(1, sizeof(MDNSResponderCallbacks));
    clientCtx->info = cbs;

    cbs->add = addCb;
    cbs->remove = removeCb;
    cbs->info = info;

    browser = CFNetServiceBrowserCreate(NULL, MDNSResponderBrowseCallback, clientCtx);

    if(!browser)
    {
        LOG(ERROR) << "CFNetServiceBrowserCreate() failed!";

        free(clientCtx);
        free(cbs);

        return;
    }

    CFNetServiceBrowserScheduleWithRunLoop(browser, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);

    if(!CFNetServiceBrowserSearchForServices(browser, domain, serviceType, NULL))
    {
        LOG(ERROR) << "CFNetServiceBrowserCreate() failed!";

        free(clientCtx);
        free(cbs);

        return;
    }
}

#endif // DARWIN