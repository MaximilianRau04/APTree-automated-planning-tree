import { useCallback, useMemo, useRef, useState } from "react";
import {
  ADD_LABELS,
  DEFAULT_DATA,
  DEFAULT_ORDER,
  DEFAULT_TITLES,
  PARAM_INSTANCES_KEY,
  PARAM_TYPES_KEY,
} from "./utils/constants";
import {
  cloneParameterInstance,
  cloneParameterType,
  createEmptyParameterInstance,
  createEmptyParameterType,
  createEmptyStructuredItem,
  generateItemId,
  generatePropertyId,
  reconcileInstanceValues,
} from "./utils/helpers";
import type {
  AppData,
  CategoryModalState,
  DataCategory,
  ModalState,
  ParameterInstance,
  ParameterInstanceModalState,
  ParameterType,
  SearchQueries,
  SidebarManager,
  StructuredItem,
  TypeModalState,
} from "./utils/types";

/**
 * builds the initial search-query map with empty strings per default category.
 * @returns initialized search query map keyed by category
 */
const createInitialSearchQueries = (): SearchQueries => {
  const initialEntries = DEFAULT_ORDER.map((key) => [key, ""] as const);
  return Object.fromEntries(initialEntries) as SearchQueries;
};

/**
 * central hook that encapsulates sidebar state, modal coordination, and item crud helpers.
 * @returns api for interacting with sidebar state and derived data
 */
export const useSidebarManager = (): SidebarManager => {
  const [data, setData] = useState<AppData>(() => ({ ...DEFAULT_DATA }));
  const typeModalRevision = useRef(0);
  const instanceModalRevision = useRef(0);

  const [categoryTitles, setCategoryTitles] = useState<Record<string, string>>(
    () => ({ ...DEFAULT_TITLES })
  );
  const [categoryOrder, setCategoryOrder] = useState<string[]>(() => [
    ...DEFAULT_ORDER,
  ]);
  const [searchQueries, setSearchQueries] = useState<SearchQueries>(
    createInitialSearchQueries
  );

  const [categoryModalState, setCategoryModalState] =
    useState<CategoryModalState>({
      isOpen: false,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });

  const [typeModalState, setTypeModalState] = useState<TypeModalState>(() => ({
    isOpen: false,
    mode: "add",
    index: null,
    initialValue: createEmptyParameterType(),
    revision: 0,
  }));

  const [instanceModalState, setInstanceModalState] =
    useState<ParameterInstanceModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: 0,
    }));

  const [modalState, setModalState] = useState<ModalState>(() => ({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: createEmptyStructuredItem(),
  }));

  const nextTypeModalRevision = () => {
    typeModalRevision.current += 1;
    return typeModalRevision.current;
  };

  const nextInstanceModalRevision = () => {
    instanceModalRevision.current += 1;
    return instanceModalRevision.current;
  };

  const parameterTypes = useMemo(() => {
    const entries = data[PARAM_TYPES_KEY] as ParameterType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const parameterTypeMap = useMemo(() => {
    const map = new Map<string, ParameterType>();
    parameterTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [parameterTypes]);

  /**
   * Opens the "add item" modal for the given category.
   * Special handling for parameter types and instances.
   *
   * @param category The category for which to add a new item.
   */
  const openAddModal = (category: DataCategory) => {
    if (category === PARAM_TYPES_KEY) {
      setTypeModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterType(),
        revision: nextTypeModalRevision(),
      });
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      if (parameterTypes.length === 0) {
        window.alert("Create a parameter type before adding instances.");
        return;
      }

      const firstType = parameterTypes[0];
      setInstanceModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterInstance(firstType),
        revision: nextInstanceModalRevision(),
      });
      return;
    }

    setModalState({
      isOpen: true,
      mode: "add",
      category,
      index: null,
      initialValue: createEmptyStructuredItem(),
    });
  };

  /**
   * Opens the edit modal for a parameter type at a given index.
   *
   * @param index The index of the parameter type to edit.
   * @param currentValue Current value of the parameter type.
   */
  const openEditParameterType = (
    index: number,
    currentValue: ParameterType
  ) => {
    setTypeModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: cloneParameterType(currentValue),
      revision: nextTypeModalRevision(),
    });
  };

  /**
   * Opens the edit modal for a parameter instance at a given index.
   *
   * @param index The index of the parameter instance to edit.
   * @param currentValue Current value of the parameter instance.
   */
  const openEditParameterInstance = (
    index: number,
    currentValue: ParameterInstance
  ) => {
    const parameterType = parameterTypeMap.get(currentValue.typeId);
    const initialEntry = cloneParameterInstance(currentValue);

    if (parameterType) {
      initialEntry.type = parameterType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        parameterType,
        currentValue.propertyValues
      );
    }

    setInstanceModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: initialEntry,
      revision: nextInstanceModalRevision(),
    });
  };

  /**
   * Opens the edit modal for a generic structured item in a category.
   * Delegates to type-specific modals for parameter types and instances.
   *
   * @param category Category key of the item.
   * @param index Index of the item to edit.
   * @param currentValue Current item value.
   */
  const openEditModal = (
    category: DataCategory,
    index: number,
    currentValue: StructuredItem
  ) => {
    if (category === PARAM_TYPES_KEY) {
      openEditParameterType(index, currentValue as ParameterType);
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      openEditParameterInstance(index, currentValue as ParameterInstance);
      return;
    }

    setModalState({
      isOpen: true,
      mode: "edit",
      category,
      index,
      initialValue: { ...currentValue },
    });
  };

  /**
   * Closes the generic item modal without saving.
   */
  const closeModal = () => {
    setModalState((prev) => ({ ...prev, isOpen: false }));
  };

  /**
   * Saves a new or edited structured item from the generic modal.
   *
   * @param value The structured item to save.
   */
  const handleSaveFromModal = (value: StructuredItem) => {
    const categoryKey = modalState.category;
    if (!categoryKey) return;

    setData((prev) => {
      const existingItems = prev[categoryKey] ?? [];
      const nextItems = [...existingItems];
      const normalized: StructuredItem = {
        ...value,
        id: value.id || generateItemId(),
      };

      if (modalState.mode === "add") {
        nextItems.push(normalized);
      } else if (modalState.mode === "edit" && modalState.index !== null) {
        nextItems[modalState.index] = normalized;
      }

      return { ...prev, [categoryKey]: nextItems };
    });

    closeModal();
  };

  /**
   * Closes the parameter type modal.
   */
  const closeTypeModal = () => {
    setTypeModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterType(),
      revision: nextTypeModalRevision(),
    });
  };

  /**
   * Saves a parameter type, updating related parameter instances as needed.
   *
   * @param value The parameter type to save.
   */
  const handleSaveParameterType = (value: ParameterType) => {
    const normalized: ParameterType = {
      ...value,
      id: value.id || createEmptyParameterType().id,
      name: value.name.trim(),
      type: value.type.trim(),
      properties: value.properties.map((property) => ({
        ...property,
        id: property.id || generatePropertyId(),
        name: property.name.trim(),
        valueType: property.valueType.trim(),
      })),
    };

    setData((prev) => {
      const existingTypes =
        (prev[PARAM_TYPES_KEY] as ParameterType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (typeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        typeModalState.mode === "edit" &&
        typeModalState.index !== null
      ) {
        nextTypes[typeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const nextInstances = existingInstances.map((instance) => {
        if (instance.typeId !== normalized.id) {
          return instance;
        }

        return {
          ...instance,
          type: normalized.name,
          propertyValues: reconcileInstanceValues(
            normalized,
            instance.propertyValues
          ),
        };
      });

      return {
        ...prev,
        [PARAM_TYPES_KEY]: nextTypes,
        [PARAM_INSTANCES_KEY]: nextInstances,
      };
    });

    closeTypeModal();
  };

  /**
   * Closes the parameter instance modal.
   */
  const closeInstanceModal = () => {
    setInstanceModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: nextInstanceModalRevision(),
    });
  };

  /**
   * Saves a parameter instance, validating against its parameter type.
   *
   * @param value The parameter instance to save.
   */
  const handleSaveParameterInstance = (value: ParameterInstance) => {
    const parameterType = parameterTypeMap.get(value.typeId);
    if (!parameterType) {
      window.alert("Select a valid parameter type.");
      return;
    }

    const sanitizedValues = parameterType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: ParameterInstance = {
      ...value,
      id: value.id || createEmptyParameterInstance().id,
      name: value.name.trim(),
      type: parameterType.name,
      typeId: parameterType.id,
      propertyValues: sanitizedValues,
    };

    setData((prev) => {
      const existing =
        (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
      const next = [...existing];

      if (instanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        instanceModalState.mode === "edit" &&
        instanceModalState.index !== null
      ) {
        next[instanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PARAM_INSTANCES_KEY]: next,
      };
    });

    closeInstanceModal();
  };

  /**
   * Deletes an item from a category after user confirmation.
   * Handles cascading deletion for dependent items (e.g., instances of a parameter type).
   *
   * @param category Category key from which to delete.
   * @param index Index of the item to delete.
   */
  const handleDeleteItem = (category: DataCategory, index: number) => {
    if (!window.confirm("Delete this item?")) {
      return;
    }

    const removedEntry = (data[category] ?? [])[index];

    setData((prev) => {
      const existingItems = prev[category] ?? [];
      const nextItems = existingItems.filter((_, i) => i !== index);
      const nextData: AppData = { ...prev, [category]: nextItems };

      if (category === PARAM_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as ParameterType;
        const existingInstances =
          (prev[PARAM_INSTANCES_KEY] as ParameterInstance[] | undefined) ?? [];
        nextData[PARAM_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      return nextData;
    });

    if (
      category === PARAM_INSTANCES_KEY &&
      instanceModalState.isOpen &&
      instanceModalState.index === index
    ) {
      closeInstanceModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      typeModalState.isOpen &&
      typeModalState.index === index
    ) {
      closeTypeModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      removedEntry &&
      instanceModalState.isOpen &&
      instanceModalState.initialValue.typeId ===
        (removedEntry as ParameterType).id
    ) {
      closeInstanceModal();
    }
  };

  /**
   * Opens the modal for creating a new category.
   */
  const openCategoryModal = () => {
    setCategoryModalState({
      isOpen: true,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

  /**
   * Opens the modal for renaming an existing category.
   *
   * @param categoryKey Key of the category to rename.
   */
  const openRenameCategoryModal = (categoryKey: DataCategory) => {
    const currentTitle = categoryTitles[categoryKey] ?? categoryKey;
    setCategoryModalState({
      isOpen: true,
      mode: "edit",
      activeKey: categoryKey,
      value: {
        ...createEmptyStructuredItem(),
        name: currentTitle,
      },
    });
  };

  /**
   * Closes the category modal without saving.
   */
  const closeCategoryModal = () => {
    setCategoryModalState({
      isOpen: false,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

  /**
   * Generates a unique key for a new category based on the display label.
   *
   * @param label Display name for the category.
   * @returns Unique key string for the category.
   */
  const createCategoryKey = (label: string) => {
    const baseKey = label
      .toLowerCase()
      .trim()
      .replace(/[^a-z0-9]+/g, "-")
      .replace(/^-+|-+$/g, "");

    const candidateBase = baseKey || "category";
    let candidate = candidateBase;
    let suffix = 1;
    const existingKeys = new Set([...categoryOrder, ...Object.keys(data)]);

    while (existingKeys.has(candidate)) {
      candidate = `${candidateBase}-${suffix++}`;
    }

    return candidate;
  };

  /**
   * Saves a new or renamed category.
   * Updates sidebar state and search queries accordingly.
   *
   * @param value The category data to save.
   */
  const handleSaveCategory = (value: StructuredItem) => {
    const displayName = value.name.trim();
    if (!displayName) {
      return;
    }

    if (categoryModalState.mode === "add") {
      const newKey = createCategoryKey(displayName);
      setData((prev) => ({ ...prev, [newKey]: [] }));
      setCategoryTitles((prev) => ({ ...prev, [newKey]: displayName }));
      setCategoryOrder((prev) => [...prev, newKey]);
      setSearchQueries((prev) => ({ ...prev, [newKey]: "" }));
    } else if (
      categoryModalState.mode === "edit" &&
      categoryModalState.activeKey
    ) {
      const activeKey = categoryModalState.activeKey;
      setCategoryTitles((prev) => ({
        ...prev,
        [activeKey]: displayName,
      }));
    }

    closeCategoryModal();
  };

  /**
   * Deletes a category and all contained items after user confirmation.
   *
   * @param categoryKey The key of the category to delete.
   */
  const handleDeleteCategory = (categoryKey: DataCategory) => {
    const displayName = categoryTitles[categoryKey] ?? categoryKey;
    if (
      !window.confirm(`Delete section "${displayName}" and all of its items?`)
    ) {
      return;
    }

    setCategoryOrder((prev) => prev.filter((key) => key !== categoryKey));
    setCategoryTitles((prev) => {
      const nextTitles = { ...prev };
      delete nextTitles[categoryKey];
      return nextTitles;
    });
    setData((prev) => {
      const nextData: AppData = { ...prev };
      delete nextData[categoryKey];
      return nextData;
    });
    setSearchQueries((prev) => {
      const nextQueries = { ...prev };
      delete nextQueries[categoryKey];
      return nextQueries;
    });
    setModalState((prev) =>
      prev.category === categoryKey
        ? {
            isOpen: false,
            mode: "add",
            category: null,
            index: null,
            initialValue: createEmptyStructuredItem(),
          }
        : prev
    );

    if (categoryKey === PARAM_TYPES_KEY) {
      closeTypeModal();
    }

    if (categoryKey === PARAM_INSTANCES_KEY) {
      closeInstanceModal();
    }

    if (categoryModalState.activeKey === categoryKey) {
      closeCategoryModal();
    }
  };

  const getItemsForCategory = useCallback(
    (category: DataCategory) => data[category] ?? [],
    [data]
  );

  const handleSearchChange = (category: DataCategory, value: string) => {
    setSearchQueries((prev) => ({ ...prev, [category]: value }));
  };

  const addLabelFor = (category: DataCategory) =>
    ADD_LABELS[category] ?? "Add Item";

  return {
    addLabelFor,
    categoryModal: categoryModalState,
    categoryOrder,
    categoryTitles,
    closeCategoryModal,
    closeInstanceModal,
    closeModal,
    closeTypeModal,
    getItemsForCategory,
    handleDeleteCategory,
    handleDeleteItem,
    handleSaveCategory,
    handleSaveFromModal,
    handleSaveParameterInstance,
    handleSaveParameterType,
    handleSearchChange,
    instanceModalState,
    modalState,
    openAddModal,
    openCategoryModal,
    openEditModal,
    openRenameCategoryModal,
    parameterTypeMap,
    parameterTypes,
    searchQueries,
    typeModalState,
  };
};
