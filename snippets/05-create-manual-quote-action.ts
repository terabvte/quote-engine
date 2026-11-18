// Snippet 5 — Next.js server action for manual quotes (TypeScript)
// Adapted from app/actions/create-manual-quote.ts in the frontend repository.
// Shortened: imports and error-formatting branches removed. No key value is present.

"use server";

import type { BuilderState } from "@/types/admin";

export async function createManualQuote(
  state: BuilderState,
  adminKey: string,
  isDraft: boolean = false,
  editId?: string,
) {
  // The UI edits nested sections; the API accepts a flat list.
  const flatItems = state.sections.flatMap((section) =>
    section.items.map((item) => ({
      Description: item.description,
      Category: section.title || "Untitled section",
      Unit: item.unit,
      Quantity: item.quantity,
      UnitPrice: item.unitPrice,
    })),
  );

  const payload = {
    Title: state.title || `Quote for ${state.client.name}`,
    ClientName: state.client.name,
    ClientEmail: state.client.email,
    ClientPhone: state.client.phone,
    ClientAddress: state.client.address,
    VatRate: state.vatRate,
    Items: flatItems,
    PublicNotes: state.publicNotes,
    InternalNotes: isDraft ? "Draft in progress" : "Created with CreaTivo",
    IsDraft: isDraft,
  };

  // Create and update share one action; the presence of editId selects POST or PUT.
  const endpoint = editId
    ? `${process.env.NEXT_PUBLIC_API_URL}/api/quotes/${editId}`
    : `${process.env.NEXT_PUBLIC_API_URL}/api/quotes/manual`;

  const response = await fetch(endpoint, {
    method: editId ? "PUT" : "POST",
    headers: {
      "Content-Type": "application/json",
      "X-Admin-Key": adminKey,
    },
    body: JSON.stringify(payload),
    cache: "no-store",
  });

  if (!response.ok) {
    return { success: false, error: await readError(response) };
  }

  const data = await response.json();
  return { success: true, pdfUrl: data.pdfUrl, quoteId: data.id };
}
