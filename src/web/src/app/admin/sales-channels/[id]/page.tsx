"use client"

import { use } from "react"
import { useQuery } from "@tanstack/react-query"
import { Card, CardContent, CardHeader, CardTitle } from "@/components/ui/card"
import { Skeleton } from "@/components/ui/skeleton"
import { SalesChannelForm } from "@/features/sales-channels/sales-channel-form"
import { SalesChannelPriceGroupsSection } from "@/features/sales-channels/sales-channel-price-groups-section"
import { salesChannelsApi } from "@/features/sales-channels/api"

interface Props { params: Promise<{ id: string }> }

export default function EditSalesChannelPage({ params }: Props) {
  const { id } = use(params)
  const query = useQuery({ queryKey: ["sales-channel", id], queryFn: () => salesChannelsApi.get(id) })

  return (
    <div className="space-y-6">
      <div>
        <h1 className="text-2xl font-semibold tracking-tight">
          {query.data ? query.data.name : "Loading sales channel…"}
        </h1>
        <p className="text-muted-foreground">Rename, toggle takeout flag, link price groups.</p>
      </div>
      {query.isLoading && <Skeleton className="h-64 w-full max-w-3xl" />}
      {query.data && (
        <div className="grid gap-6 lg:grid-cols-2">
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Basics</CardTitle>
            </CardHeader>
            <CardContent>
              <SalesChannelForm initial={query.data} />
            </CardContent>
          </Card>
          <Card>
            <CardHeader>
              <CardTitle className="text-base">Price groups</CardTitle>
            </CardHeader>
            <CardContent>
              <SalesChannelPriceGroupsSection channelId={query.data.id} attachedIds={query.data.priceGroupIds} />
            </CardContent>
          </Card>
        </div>
      )}
    </div>
  )
}
