export type PagedResult<T> = {
  items: T[];
  page: number;
  pageSize: number;
  total: number;
};

export type AccountType = "Staff" | "Guest";

export type LoginResponse = {
  accessToken: string;
  expiresAt: string;
  accountType: AccountType;
};

export type Me = {
  userId: string;
  email: string;
  firstName: string;
  lastName: string;
  accountType: AccountType;
  outletId: string | null;
  roles: string[];
  permissions: string[];
};

export type Outlet = {
  id: string;
  displayName: string;
  streetAddress: string;
  phone: string;
  timeZone: string;
  currency: string;
  logoUrl: string | null;
};

export type Tag = {
  id: string;
  name: string;
  imageUrl: string | null;
  createdAt: string;
};

export type Allergen = { id: string; name: string };

export type ImageInfo = { id: string; url: string };

export type MenuItem = {
  id: string;
  name: string;
  barcode: string | null;
  price: number;
  isEventPrice: boolean;
};

export type MenuItemDetail = {
  id: string;
  name: string;
  description: string | null;
  price: number;
  isEventPrice: boolean;
  allergenIds: string[];
  images: ImageInfo[];
};

export type SalesChannel = {
  id: string;
  name: string;
  isTakeout: boolean;
};

export type Table = { id: string; name: string; outletId: string };

export type EventStatus = "Draft" | "Published" | "Closed" | "Cancelled";

export type EventItem = {
  id: string;
  name: string;
  status: EventStatus;
  createdAt: string;
};

export type EventDetail = {
  id: string;
  name: string;
  description: string | null;
  imageUrl: string | null;
  productListId: string | null;
  priceGroupId: string | null;
  status: EventStatus;
  publishedAt: string | null;
  closedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  days: { id: string; date: string }[];
  createdAt: string;
};

export type OrderStatus = "Placed" | "Closed" | "Cancelled";

export type OrderItem = {
  id: string;
  outletId: string;
  userId: string | null;
  status: OrderStatus;
  createdAt: string;
  closedAt: string | null;
};

export type OrderLine = {
  id: string;
  productId: string;
  productName: string;
  quantity: number;
  netPerOne: number;
  vatPerOne: number;
};

export type OrderDetail = {
  id: string;
  outletId: string;
  tableId: string | null;
  salesChannelId: string | null;
  userId: string | null;
  eventId: string | null;
  promotionCodeId: string | null;
  discount: number;
  loyaltyPointsUsed: number;
  status: OrderStatus;
  closedAt: string | null;
  cancelledAt: string | null;
  cancellationReason: string | null;
  lines: OrderLine[];
  createdAt: string;
};

export type LoyaltyPointLog = {
  id: string;
  points: number;
  reason: string | null;
  createdAt: string;
};

export type Favorite = {
  productId: string;
  productName: string;
  description: string | null;
};

export type ValidatePromoResponse = {
  valid: boolean;
  discountPercentage: number | null;
  reason: string | null;
};
