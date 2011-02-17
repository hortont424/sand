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
 * THIS SOFTWARE IS PROVIDED BY TIM HORTON "AS IS" AND ANY EXPRESS OR IMPLIED
 * WARRANTIES, INCLUDING, BUT NOT LIMITED TO, THE IMPLIED WARRANTIES OF
 * MERCHANTABILITY AND FITNESS FOR A PARTICULAR PURPOSE ARE DISCLAIMED. IN NO EVENT
 * SHALL TIM HORTON OR CONTRIBUTORS BE LIABLE FOR ANY DIRECT, INDIRECT,
 * INCIDENTAL, SPECIAL, EXEMPLARY, OR CONSEQUENTIAL DAMAGES (INCLUDING, BUT NOT
 * LIMITED TO, PROCUREMENT OF SUBSTITUTE GOODS OR SERVICES; LOSS OF USE, DATA, OR
 * PROFITS; OR BUSINESS INTERRUPTION) HOWEVER CAUSED AND ON ANY THEORY OF
 * LIABILITY, WHETHER IN CONTRACT, STRICT LIABILITY, OR TORT (INCLUDING
 * NEGLIGENCE OR OTHERWISE) ARISING IN ANY WAY OUT OF THE USE OF THIS SOFTWARE,
 * EVEN IF ADVISED OF THE POSSIBILITY OF SUCH DAMAGE.
 */

#include "mdns.h"

// TODO: ifdef darwin?

#include <CoreServices/CoreServices.h>
#include <glog/logging.h>

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

    CFNetServiceResolveWithTimeout(svc, 3, NULL);
    //char type[1024];

    //CFStringGetCString(CFNetServiceGetAddressing(svc), type, sizeof(type), CFStringGetSystemEncoding());

    if(flags & kCFNetServiceFlagRemove)
    {
        LOG(INFO) << "Remove service on port: " << CFNetServiceGetPortNumber(svc);
    }
    else
    {
        LOG(INFO) << "Add service on port: " << CFNetServiceGetPortNumber(svc);
    }
}

#pragma mark Public Functions

void MDNSResponderTick()
{
    CFRunLoopRunInMode(kCFRunLoopDefaultMode, 0, true);
}

void MDNSRegisterService(MDNSService * service)
{
    CFNetServiceRef svc;
    CFNetServiceClientContext * clientCtx;
    CFStringRef domain, serviceType, name;

    domain = CFSTR("local");
    serviceType = CFStringCreateWithCString(NULL, service->type, CFStringGetSystemEncoding());
    name = CFStringCreateWithCString(NULL, service->name, CFStringGetSystemEncoding());

    LOG(INFO) << "Registering mDNS service " << service->type << " on port " << service->port;

    svc = CFNetServiceCreate(NULL, domain, serviceType, name, service->port);

    if(!svc)
    {
        LOG(ERROR) << "CFNetServiceCreate() failed!";

        CFRelease(serviceType);
        CFRelease(name);

        return;
    }

    clientCtx = (CFNetServiceClientContext *)calloc(1, sizeof(CFNetServiceClientContext));

    if(!CFNetServiceSetClient(svc, MDNSResponderRegistrationCallback, clientCtx))
    {
        LOG(ERROR) << "CFNetServiceSetClient() failed!";

        free(clientCtx);

        CFRelease(serviceType);
        CFRelease(name);

        return;
    }

    CFNetServiceScheduleWithRunLoop(svc, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);

    if(!CFNetServiceRegisterWithOptions(svc, 0, NULL))
    {
        LOG(ERROR) << "CFNetServiceRegisterWithOptions() failed!";

        CFNetServiceSetClient(svc, NULL, NULL);

        free(clientCtx);

        CFRelease(serviceType);
        CFRelease(name);

        return;
    }
}

void MDNSBrowseService(const char * _serviceType)
{
    CFNetServiceBrowserRef browser;
    CFNetServiceClientContext * clientCtx;
    CFStringRef domain, serviceType, name;

    domain = CFSTR("local");
    serviceType = CFStringCreateWithCString(NULL, _serviceType, CFStringGetSystemEncoding());

    clientCtx = (CFNetServiceClientContext *)calloc(1, sizeof(CFNetServiceClientContext));

    browser = CFNetServiceBrowserCreate(NULL, MDNSResponderBrowseCallback, clientCtx);

    if(!browser)
    {
        LOG(ERROR) << "CFNetServiceBrowserCreate() failed!";

        CFRelease(serviceType);

        free(clientCtx);

        return;
    }

    CFNetServiceBrowserScheduleWithRunLoop(browser, CFRunLoopGetCurrent(), kCFRunLoopDefaultMode);

    if(!CFNetServiceBrowserSearchForServices(browser, domain, serviceType, NULL))
    {
        LOG(ERROR) << "CFNetServiceBrowserCreate() failed!";

        CFRelease(serviceType);

        free(clientCtx);

        return;
    }
}