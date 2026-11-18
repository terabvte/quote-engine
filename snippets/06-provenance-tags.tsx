// Snippet 6 — surfacing price provenance in the UI (TSX/TypeScript)
// Adapted from components/QuoteGenerator.tsx in the frontend repository.
// This helper runs before rendering each quote line.

type ItemMeta = {
  cleanDescription: string;
  tagType: "none" | "manual" | "ai" | "web";
  reasoning?: string;
};

const processItem = (originalDescription: string): ItemMeta => {
  // A line priced by the model fallback carries a visible tag.
  const webMatch = originalDescription.match(/\[🌍 STIMA WEB: (.*?)\]/);
  if (webMatch) {
    return {
      cleanDescription: originalDescription.replace(webMatch[0], "").trim(),
      tagType: "web",
      reasoning: webMatch[1],
    };
  }

  const aiMatch = originalDescription.match(/\[⚡ STIMA AI: (.*?)\]/);
  if (aiMatch) {
    return {
      cleanDescription: originalDescription.replace(aiMatch[0], "").trim(),
      tagType: "ai",
      reasoning: aiMatch[1],
    };
  }

  // Older quotes used a single generic tag.
  const legacyTag = "[⚠️ Prezzo Stimato]";
  if (originalDescription.includes(legacyTag)) {
    return {
      cleanDescription: originalDescription.replace(legacyTag, "").trim(),
      tagType: "manual",
    };
  }

  return { cleanDescription: originalDescription, tagType: "none" };
};
