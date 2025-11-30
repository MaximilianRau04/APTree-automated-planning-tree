import { useCallback, useMemo, useRef, useState } from "react";
import {
  ADD_LABELS,
  DEFAULT_DATA,
  DEFAULT_ORDER,
  DEFAULT_TITLES,
  ACTION_INSTANCES_KEY,
  ACTION_TYPES_KEY,
  PARAM_INSTANCES_KEY,
  PARAM_TYPES_KEY,
  PREDICATE_INSTANCES_KEY,
  PREDICATE_TYPES_KEY,
} from "./utils/constants";
import {
  cloneActionInstance,
  cloneActionType,
  cloneParameterInstance,
  cloneParameterType,
  clonePredicateInstance,
  clonePredicateType,
  createEmptyActionInstance,
  createEmptyActionType,
  createEmptyParameterInstance,
  createEmptyParameterType,
  createEmptyPredicateInstance,
  createEmptyPredicateType,
  createEmptyStructuredItem,
  generateItemId,
  generatePropertyId,
  reconcileInstanceValues,
} from "./utils/helpers";
import type {
  ActionInstance,
  ActionInstanceModalState,
  ActionType,
  ActionTypeModalState,
  AppData,
  CategoryModalState,
  DataCategory,
  ModalState,
  ParameterInstance,
  ParameterInstanceModalState,
  ParameterType,
  PredicateInstance,
  PredicateInstanceModalState,
  PredicateType,
  PredicateTypeModalState,
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
  const parameterTypeRevision = useRef(0);
  const predicateTypeRevision = useRef(0);
  const actionTypeRevision = useRef(0);
  const parameterInstanceRevision = useRef(0);
  const predicateInstanceRevision = useRef(0);
  const actionInstanceRevision = useRef(0);

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

  const [parameterTypeModalState, setParameterTypeModalState] =
    useState<TypeModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterType(),
      revision: 0,
    }));

  const [predicateTypeModalState, setPredicateTypeModalState] =
    useState<PredicateTypeModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyPredicateType(),
      revision: 0,
    }));

  const [actionTypeModalState, setActionTypeModalState] =
    useState<ActionTypeModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyActionType(),
      revision: 0,
    }));

  const [parameterInstanceModalState, setParameterInstanceModalState] =
    useState<ParameterInstanceModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: 0,
    }));

  const [predicateInstanceModalState, setPredicateInstanceModalState] =
    useState<PredicateInstanceModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyPredicateInstance(),
      revision: 0,
    }));

  const [actionInstanceModalState, setActionInstanceModalState] =
    useState<ActionInstanceModalState>(() => ({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyActionInstance(),
      revision: 0,
    }));

  const [modalState, setModalState] = useState<ModalState>(() => ({
    isOpen: false,
    mode: "add",
    category: null,
    index: null,
    initialValue: createEmptyStructuredItem(),
  }));

  const nextParameterTypeRevision = () => {
    parameterTypeRevision.current += 1;
    return parameterTypeRevision.current;
  };

  const nextPredicateTypeRevision = () => {
    predicateTypeRevision.current += 1;
    return predicateTypeRevision.current;
  };

  const nextActionTypeRevision = () => {
    actionTypeRevision.current += 1;
    return actionTypeRevision.current;
  };

  const nextParameterInstanceRevision = () => {
    parameterInstanceRevision.current += 1;
    return parameterInstanceRevision.current;
  };

  const nextPredicateInstanceRevision = () => {
    predicateInstanceRevision.current += 1;
    return predicateInstanceRevision.current;
  };

  const nextActionInstanceRevision = () => {
    actionInstanceRevision.current += 1;
    return actionInstanceRevision.current;
  };

  const parameterTypes = useMemo(() => {
    const entries = data[PARAM_TYPES_KEY] as ParameterType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const predicateTypes = useMemo(() => {
    const entries = data[PREDICATE_TYPES_KEY] as PredicateType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const actionTypes = useMemo(() => {
    const entries = data[ACTION_TYPES_KEY] as ActionType[] | undefined;
    return entries ? [...entries] : [];
  }, [data]);

  const parameterTypeMap = useMemo(() => {
    const map = new Map<string, ParameterType>();
    parameterTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [parameterTypes]);

  const predicateTypeMap = useMemo(() => {
    const map = new Map<string, PredicateType>();
    predicateTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [predicateTypes]);

  const actionTypeMap = useMemo(() => {
    const map = new Map<string, ActionType>();
    actionTypes.forEach((entry) => {
      map.set(entry.id, entry);
    });
    return map;
  }, [actionTypes]);

  /**
   * Opens the "add item" modal for the given category.
   * Special handling for typed categories.
   *
   * @param category The category for which to add a new item.
   */
  const openAddModal = (category: DataCategory) => {
    if (category === PARAM_TYPES_KEY) {
      setParameterTypeModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterType(),
        revision: nextParameterTypeRevision(),
      });
      return;
    }

    if (category === PREDICATE_TYPES_KEY) {
      setPredicateTypeModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyPredicateType(),
        revision: nextPredicateTypeRevision(),
      });
      return;
    }

    if (category === ACTION_TYPES_KEY) {
      setActionTypeModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyActionType(),
        revision: nextActionTypeRevision(),
      });
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      if (parameterTypes.length === 0) {
        window.alert("Create a parameter type before adding instances.");
        return;
      }

      const defaultType = parameterTypes[0];
      setParameterInstanceModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyParameterInstance(defaultType),
        revision: nextParameterInstanceRevision(),
      });
      return;
    }

    if (category === PREDICATE_INSTANCES_KEY) {
      if (predicateTypes.length === 0) {
        window.alert("Create a predicate type before adding instances.");
        return;
      }

      const defaultType = predicateTypes[0];
      setPredicateInstanceModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyPredicateInstance(defaultType),
        revision: nextPredicateInstanceRevision(),
      });
      return;
    }

    if (category === ACTION_INSTANCES_KEY) {
      if (actionTypes.length === 0) {
        window.alert("Create an action type before adding instances.");
        return;
      }

      const defaultType = actionTypes[0];
      setActionInstanceModalState({
        isOpen: true,
        mode: "add",
        index: null,
        initialValue: createEmptyActionInstance(defaultType),
        revision: nextActionInstanceRevision(),
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

  const openEditParameterType = (
    index: number,
    currentValue: ParameterType
  ) => {
    setParameterTypeModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: cloneParameterType(currentValue),
      revision: nextParameterTypeRevision(),
    });
  };

  const openEditPredicateType = (
    index: number,
    currentValue: PredicateType
  ) => {
    setPredicateTypeModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: clonePredicateType(currentValue),
      revision: nextPredicateTypeRevision(),
    });
  };

  const openEditActionType = (index: number, currentValue: ActionType) => {
    setActionTypeModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: cloneActionType(currentValue),
      revision: nextActionTypeRevision(),
    });
  };

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

    setParameterInstanceModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: initialEntry,
      revision: nextParameterInstanceRevision(),
    });
  };

  const openEditPredicateInstance = (
    index: number,
    currentValue: PredicateInstance
  ) => {
    const predicateType = predicateTypeMap.get(currentValue.typeId);
    const initialEntry = clonePredicateInstance(currentValue);

    if (predicateType) {
      initialEntry.type = predicateType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        predicateType,
        currentValue.propertyValues
      );
    }

    setPredicateInstanceModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: initialEntry,
      revision: nextPredicateInstanceRevision(),
    });
  };

  const openEditActionInstance = (
    index: number,
    currentValue: ActionInstance
  ) => {
    const actionType = actionTypeMap.get(currentValue.typeId);
    const initialEntry = cloneActionInstance(currentValue);

    if (actionType) {
      initialEntry.type = actionType.name;
      initialEntry.propertyValues = reconcileInstanceValues(
        actionType,
        currentValue.propertyValues
      );
    }

    setActionInstanceModalState({
      isOpen: true,
      mode: "edit",
      index,
      initialValue: initialEntry,
      revision: nextActionInstanceRevision(),
    });
  };

  /**
   * Opens the edit modal for a generic structured item in a category.
   * Delegates to type-specific modals for typed categories.
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

    if (category === PREDICATE_TYPES_KEY) {
      openEditPredicateType(index, currentValue as PredicateType);
      return;
    }

    if (category === ACTION_TYPES_KEY) {
      openEditActionType(index, currentValue as ActionType);
      return;
    }

    if (category === PARAM_INSTANCES_KEY) {
      openEditParameterInstance(index, currentValue as ParameterInstance);
      return;
    }

    if (category === PREDICATE_INSTANCES_KEY) {
      openEditPredicateInstance(index, currentValue as PredicateInstance);
      return;
    }

    if (category === ACTION_INSTANCES_KEY) {
      openEditActionInstance(index, currentValue as ActionInstance);
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

  const closeParameterTypeModal = () => {
    setParameterTypeModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterType(),
      revision: nextParameterTypeRevision(),
    });
  };

  const closePredicateTypeModal = () => {
    setPredicateTypeModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyPredicateType(),
      revision: nextPredicateTypeRevision(),
    });
  };

  const closeActionTypeModal = () => {
    setActionTypeModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyActionType(),
      revision: nextActionTypeRevision(),
    });
  };

  const closeParameterInstanceModal = () => {
    setParameterInstanceModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyParameterInstance(),
      revision: nextParameterInstanceRevision(),
    });
  };

  const closePredicateInstanceModal = () => {
    setPredicateInstanceModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyPredicateInstance(),
      revision: nextPredicateInstanceRevision(),
    });
  };

  const closeActionInstanceModal = () => {
    setActionInstanceModalState({
      isOpen: false,
      mode: "add",
      index: null,
      initialValue: createEmptyActionInstance(),
      revision: nextActionInstanceRevision(),
    });
  };

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

      if (parameterTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        parameterTypeModalState.mode === "edit" &&
        parameterTypeModalState.index !== null
      ) {
        nextTypes[parameterTypeModalState.index] = normalized;
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

    closeParameterTypeModal();
  };

  const handleSavePredicateType = (value: PredicateType) => {
    const normalized: PredicateType = {
      ...value,
      id: value.id || createEmptyPredicateType().id,
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
        (prev[PREDICATE_TYPES_KEY] as PredicateType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (predicateTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        predicateTypeModalState.mode === "edit" &&
        predicateTypeModalState.index !== null
      ) {
        nextTypes[predicateTypeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
        [];
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
        [PREDICATE_TYPES_KEY]: nextTypes,
        [PREDICATE_INSTANCES_KEY]: nextInstances,
      };
    });

    closePredicateTypeModal();
  };

  const handleSaveActionType = (value: ActionType) => {
    const normalized: ActionType = {
      ...value,
      id: value.id || createEmptyActionType().id,
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
        (prev[ACTION_TYPES_KEY] as ActionType[] | undefined) ?? [];
      const nextTypes = [...existingTypes];

      if (actionTypeModalState.mode === "add") {
        nextTypes.push(normalized);
      } else if (
        actionTypeModalState.mode === "edit" &&
        actionTypeModalState.index !== null
      ) {
        nextTypes[actionTypeModalState.index] = normalized;
      }

      const existingInstances =
        (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
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
        [ACTION_TYPES_KEY]: nextTypes,
        [ACTION_INSTANCES_KEY]: nextInstances,
      };
    });

    closeActionTypeModal();
  };

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

      if (parameterInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        parameterInstanceModalState.mode === "edit" &&
        parameterInstanceModalState.index !== null
      ) {
        next[parameterInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PARAM_INSTANCES_KEY]: next,
      };
    });

    closeParameterInstanceModal();
  };

  const handleSavePredicateInstance = (value: PredicateInstance) => {
    const predicateType = predicateTypeMap.get(value.typeId);
    if (!predicateType) {
      window.alert("Select a valid predicate type.");
      return;
    }

    const sanitizedValues = predicateType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: PredicateInstance = {
      ...value,
      id: value.id || createEmptyPredicateInstance().id,
      name: value.name.trim(),
      type: predicateType.name,
      typeId: predicateType.id,
      propertyValues: sanitizedValues,
      isNegated: value.isNegated ?? false,
    };

    setData((prev) => {
      const existing =
        (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
        [];
      const next = [...existing];

      if (predicateInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        predicateInstanceModalState.mode === "edit" &&
        predicateInstanceModalState.index !== null
      ) {
        next[predicateInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [PREDICATE_INSTANCES_KEY]: next,
      };
    });

    closePredicateInstanceModal();
  };

  const handleSaveActionInstance = (value: ActionInstance) => {
    const actionType = actionTypeMap.get(value.typeId);
    if (!actionType) {
      window.alert("Select a valid action type.");
      return;
    }

    const sanitizedValues = actionType.properties.reduce<
      Record<string, string>
    >((acc, property) => {
      const rawValue = value.propertyValues?.[property.id] ?? "";
      acc[property.id] = rawValue.trim();
      return acc;
    }, {});

    const normalized: ActionInstance = {
      ...value,
      id: value.id || createEmptyActionInstance().id,
      name: value.name.trim(),
      type: actionType.name,
      typeId: actionType.id,
      propertyValues: sanitizedValues,
    };

    setData((prev) => {
      const existing =
        (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
      const next = [...existing];

      if (actionInstanceModalState.mode === "add") {
        next.push(normalized);
      } else if (
        actionInstanceModalState.mode === "edit" &&
        actionInstanceModalState.index !== null
      ) {
        next[actionInstanceModalState.index] = normalized;
      }

      return {
        ...prev,
        [ACTION_INSTANCES_KEY]: next,
      };
    });

    closeActionInstanceModal();
  };

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

      if (category === PREDICATE_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as PredicateType;
        const existingInstances =
          (prev[PREDICATE_INSTANCES_KEY] as PredicateInstance[] | undefined) ??
          [];
        nextData[PREDICATE_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      if (category === ACTION_TYPES_KEY && removedEntry) {
        const removedType = removedEntry as ActionType;
        const existingInstances =
          (prev[ACTION_INSTANCES_KEY] as ActionInstance[] | undefined) ?? [];
        nextData[ACTION_INSTANCES_KEY] = existingInstances.filter(
          (instance) => instance.typeId !== removedType.id
        );
      }

      return nextData;
    });

    if (
      category === PARAM_INSTANCES_KEY &&
      parameterInstanceModalState.isOpen &&
      parameterInstanceModalState.index === index
    ) {
      closeParameterInstanceModal();
    }

    if (
      category === PREDICATE_INSTANCES_KEY &&
      predicateInstanceModalState.isOpen &&
      predicateInstanceModalState.index === index
    ) {
      closePredicateInstanceModal();
    }

    if (
      category === ACTION_INSTANCES_KEY &&
      actionInstanceModalState.isOpen &&
      actionInstanceModalState.index === index
    ) {
      closeActionInstanceModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      parameterTypeModalState.isOpen &&
      parameterTypeModalState.index === index
    ) {
      closeParameterTypeModal();
    }

    if (
      category === PREDICATE_TYPES_KEY &&
      predicateTypeModalState.isOpen &&
      predicateTypeModalState.index === index
    ) {
      closePredicateTypeModal();
    }

    if (
      category === ACTION_TYPES_KEY &&
      actionTypeModalState.isOpen &&
      actionTypeModalState.index === index
    ) {
      closeActionTypeModal();
    }

    if (
      category === PARAM_TYPES_KEY &&
      removedEntry &&
      parameterInstanceModalState.isOpen &&
      parameterInstanceModalState.initialValue.typeId ===
        (removedEntry as ParameterType).id
    ) {
      closeParameterInstanceModal();
    }

    if (
      category === PREDICATE_TYPES_KEY &&
      removedEntry &&
      predicateInstanceModalState.isOpen &&
      predicateInstanceModalState.initialValue.typeId ===
        (removedEntry as PredicateType).id
    ) {
      closePredicateInstanceModal();
    }

    if (
      category === ACTION_TYPES_KEY &&
      removedEntry &&
      actionInstanceModalState.isOpen &&
      actionInstanceModalState.initialValue.typeId ===
        (removedEntry as ActionType).id
    ) {
      closeActionInstanceModal();
    }
  };

  const openCategoryModal = () => {
    setCategoryModalState({
      isOpen: true,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

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

  const closeCategoryModal = () => {
    setCategoryModalState({
      isOpen: false,
      mode: "add",
      activeKey: null,
      value: createEmptyStructuredItem(),
    });
  };

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
      closeParameterTypeModal();
    }

    if (categoryKey === PREDICATE_TYPES_KEY) {
      closePredicateTypeModal();
    }

    if (categoryKey === ACTION_TYPES_KEY) {
      closeActionTypeModal();
    }

    if (categoryKey === PARAM_INSTANCES_KEY) {
      closeParameterInstanceModal();
    }

    if (categoryKey === PREDICATE_INSTANCES_KEY) {
      closePredicateInstanceModal();
    }

    if (categoryKey === ACTION_INSTANCES_KEY) {
      closeActionInstanceModal();
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
    closeParameterInstanceModal,
    closePredicateInstanceModal,
    closeActionInstanceModal,
    closeModal,
    closeParameterTypeModal,
    closePredicateTypeModal,
    closeActionTypeModal,
    getItemsForCategory,
    handleDeleteCategory,
    handleDeleteItem,
    handleSaveCategory,
    handleSaveFromModal,
    handleSaveParameterInstance,
    handleSavePredicateInstance,
    handleSaveActionInstance,
    handleSaveParameterType,
    handleSavePredicateType,
    handleSaveActionType,
    handleSearchChange,
    parameterInstanceModalState,
    predicateInstanceModalState,
    actionInstanceModalState,
    modalState,
    openAddModal,
    openCategoryModal,
    openEditModal,
    openRenameCategoryModal,
    parameterTypeMap,
    parameterTypes,
    predicateTypeMap,
    predicateTypes,
    actionTypeMap,
    actionTypes,
    searchQueries,
    parameterTypeModalState,
    predicateTypeModalState,
    actionTypeModalState,
  };
};
