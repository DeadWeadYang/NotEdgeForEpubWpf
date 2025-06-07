function checkSubClass(parent, child) {
    return typeof child === 'function' &&
        typeof parent === 'function' &&
        child !== parent &&
        parent.prototype.isPrototypeOf(child.prototype);
}
function safePropertySet(target, key, value) {
    const descriptor = Object.getOwnPropertyDescriptor(target, key);

    if (descriptor) {
        // If there's a setter, or if the property is a writable data property, set the value.
        if (typeof descriptor.set === 'function' || descriptor.writable) {
            target[key] = value;
        } else {
            // Otherwise, skip assignment.
            console.warn(`Skipping assignment for "${key}": read-only property.`);
        }
    } else {
        // If the property doesn't exist on the object, it's safe to define it.
        target[key] = value;
    }
}
class Validatable {
    static get metaPropertyInfo() {
        return {};
    }
  /**
   * Recursively creates an instance of the class from a plain object.
   * It automatically assigns properties and, if a property is a nested object
   * with its own fromObject, applies the conversion recursively.
   * Returns the validated instance or null if validation fails.
   *
   * @param {object} obj - A plain object (e.g. parsed from JSON).
   * @returns {object|null} - A validated instance of the class, or null.
   */
  static fromObject(obj) {
    const instance = new this();
    for (const key of Object.keys(obj)) {
      const value = obj[key];

      // If the target already has a nested object that supports fromObject,
      // use recursion to assign and validate that nested object.
        if (
            instance.hasOwnProperty(key) &&
            instance[key] !== null &&
            typeof instance[key] === "object" &&
            typeof instance[key].constructor.fromObject === "function" &&
            value !== null &&
            typeof value === "object"
        ) {
            const nested = instance[key].constructor.fromObject(value);
            if (nested === null) {
                // Nested validation failed.
                return null;
            }
            //instance[key] = nested;
            safePropertySet(instance, key, nested);

        }
        else if (checkSubClass(Validatable, instance.constructor.metaPropertyInfo[key])) {
            const nested = instance.constructor.metaPropertyInfo[key].fromObject(value);
            safePropertySet(instance, key, nested);
        }
        else {
          //instance[key] = value;
          safePropertySet(instance, key, value);
      }
    }

    return instance;
  }
}


class AnnoBody extends Validatable {
    static get type() { return "TextualBody"; }
    get type() { return this.constructor.type; }
    value; format;
    color = "yellow";
    highlight; language; textDirection; keyword;
    static get metaPropertyInfo() {
        return {value:String,color:String}
    }
    constructor(value) {
        super();
        if (typeof value !== "string") {
            console.warn("Invalid parameter of TextualBody: expected a string.");
        } else this.value = value;
    }
    toJSON() {
        return {
            "type": this.type,
            ...this
        }
    }
    static fromObject(obj) {
        if (obj.type !== this.type
            || (typeof obj.value !== "string")
        ) return null;
        return super.fromObject(obj);
    }

}


class RangeOffsetSelector extends Validatable {
    static get type() { return "RangeOffsetSelector"; }
    get type() { return this.constructor.type; }
    value;
    static get metaPropertyInfo() {
        return { value: Number }
    }
    constructor(value) {
        super();
        if (
            (typeof value !== "number")
            // && ( (typeof value !== "string")
            //     || (value!=="before" || value!=="after")
            // )
        ) {
            console.warn("Invalid parameter of RangeOffsetSelector: expected a number.");
        } else this.value = value;
    }
    toJSON() {
        return {
            "type": this.type,
            ...this
        }
    }
    static fromObject(obj) {
        if (obj.type !== this.type
            || (
                (typeof obj.value !== "number")
                // && ( (typeof value !== "string")
                //     || (value!=="before" && value!=="after")
                // )
            )
        ) return null;
        return super.fromObject(obj);
    }
}
class XPathSelector extends Validatable {
    static get type() { return "XPathSelector"; }
    get type() { return this.constructor.type; }
    /**
     * @type {string}
     */
    value;
    refinedBy;
    static get metaPropertyInfo() {
        return { value: String, refinedBy: RangeOffsetSelector }
    }
    constructor(value) {
        super();
        if (typeof value !== "string") {
            console.warn("Invalid parameter of XPathSelector: expected a string.");
        } else this.value = value;
    }
    toJSON() {
        return {
            "type": this.type,
            ...this
        }
    }
    static fromObject(obj) {
        if (obj.type !== this.type
            || (typeof obj.value !== "string")
        ) return null;
        return super.fromObject(obj);
    }
}

class RangeSelector extends Validatable {
    static get type() { return "RangeSelector"; }
    get type() { return this.constructor.type; }
    startSelector; endSelector;
    static get metaPropertyInfo() {
        return { startSelector: XPathSelector, startSelector: XPathSelector }
    }
    toJSON() {
        return {
            "type": this.type,
            ...this
        }
    }
    static fromObject(obj) {
        if (obj.type !== this.type
            || obj.startSelector === undefined
            || obj.endSelector === undefined
        ) return null;
        return super.fromObject(obj);
    }
}

class AnnoTarget extends Validatable {
    /**
     * @type {string}
     */
    source;
    selector;
    meta;
    static get metaPropertyInfo() {
        return {
            source: String, selector: RangeSelector
        }
    }
    static fromObject(obj) {
        if (typeof obj.source !== "string") return null;
        return super.fromObject(obj);
    }
}

class Annotation extends Validatable{
    static get ["@context"](){return "http://www.w3.org/ns/anno.jsonld";}
    static get type(){return "Annotation";};
    get ["@context"](){return this.constructor["@context"];}
    get type(){return this.constructor.type;};
    /**
     * @type {string}
     */
    id;
    motivation;created;modified;creator;
    /**
     * @type {AnnoTarget}
     */
    target;
    /**
     * @type {AnnoBody}
     */
    body;
    static get metaPropertyInfo() {
        return { id: String, motivation: String, created: String, modified: String, target: AnnoTarget };
    }
    toJSON(){
        return {
            "@context":this["@context"],
            "type":this.type,
            ...this
        }
    }
    static fromObject(obj){
        if( obj["@context"]!==this["@context"]
            || obj.type!==this.type
            || (typeof obj.id !== "string")
            || (typeof obj.created !== "string")
            || obj.target===undefined
        )return null;
        return super.fromObject(obj);
    }
}

class AnnotationRangePair{
  /**
   * @type {Annotation}
   */
  annotation;
  /**
   * @type {Range}
   */
  range;
}


/**
 * Returns an absolute, namespace‑agnostic XPath for a node that can serve as a Range container.
 * Supported node types:
 *   - Element nodes (nodeType === 1)
 *   - Text nodes (nodeType === 3)
 *   - CDATA section nodes (nodeType === 4)
 *
 * In particular, for element nodes, instead of returning a path like "/HTML[1]/BODY[1]",
 * we return "/*[local-name()='HTML'][1]/*[local-name()='BODY'][1]", which works whether or not
 * the document is in a namespace.
 *
 * @param {Node} node - The node to convert to an XPath.
 * @returns {string} The absolute XPath of the node.
 * @throws {Error} if the node is not acceptable.
 */
function getXPath(node) {
  if (!node) {
    throw new Error("Invalid node provided.");
  }
  
  // The document node is our base.
  if (node.nodeType === Node.DOCUMENT_NODE) {
    return "";
  }
  
  // We only accept Element, Text, or CDATA nodes.
  if (
    node.nodeType !== Node.ELEMENT_NODE &&
    node.nodeType !== Node.TEXT_NODE &&
    node.nodeType !== Node.CDATA_SECTION_NODE
  ) {
    throw new Error(
      "Node type " + node.nodeType + " cannot be used as a Range container in HTML/XHTML."
    );
  }
  
  // --- Handle Text nodes and CDATA sections ---
  if (node.nodeType === Node.TEXT_NODE || node.nodeType === Node.CDATA_SECTION_NODE) {
    if (!node.parentNode) {
      throw new Error("Detached text/CDATA node cannot be reliably located.");
    }
    // Get the parent's XPath.
    const parentXPath = getXPath(node.parentNode);
    // Filter parent's childNodes to include text nodes and CDATA nodes.
    const textSiblings = Array.from(node.parentNode.childNodes).filter(
      n => n.nodeType === Node.TEXT_NODE || n.nodeType === Node.CDATA_SECTION_NODE
    );
    // XPath indices are 1-indexed.
    const index = textSiblings.indexOf(node) + 1;
    return parentXPath + "/text()[" + index + "]";
  }
  
  // --- Handle Element nodes ---
  if (node.nodeType === Node.ELEMENT_NODE) {
    // Shortcut if the element has an ID.
    if (node.id) {
      return `//*[@id="${node.id}"]`;
    }
    
    let parentXPath = "";
    if (node.parentNode && node.parentNode.nodeType !== Node.DOCUMENT_NODE) {
      parentXPath = getXPath(node.parentNode);
    }
    
    // For namespace‐agnostic matching, use local-name() in the XPath.
    // Compute siblings by comparing local names.
    let siblings;
    if (node.parentNode) {
      siblings = Array.from(node.parentNode.children).filter(
        sib => (sib.localName || sib.tagName) === (node.localName || node.tagName)
      );
    } else {
      siblings = [node];
    }
    const index = siblings.indexOf(node) + 1;
    const name = node.localName || node.tagName;
    return (parentXPath === "" ? "" : parentXPath) + "/*[local-name()='" + name + "'][" + index + "]";
  }
  
  // Should not be reached.
  throw new Error("Unhandled node type: " + node.nodeType);
}

/**
 * Serializes a Range object by saving:
 *   - The XPath of the start container.
 *   - The start offset.
 *   - The XPath of the end container.
 *   - The end offset.
 *
 * @param {Range} range - The Range object to serialize.
 * @returns {string} A JSON string representing the Range.
 */
function serializeRange(range) {
  return JSON.stringify({
    startContainerXPath: getXPath(range.startContainer),
    startOffset: range.startOffset,
    endContainerXPath: getXPath(range.endContainer),
    endOffset: range.endOffset
  });
}
/**
 * 
 * @param {Range} range 
 * @returns {RangeSelector}
 */
function rangeToSelector(range){
    let startContainerXPath= getXPath(range.startContainer);
    let startOffset= range.startOffset;
    let endContainerXPath= getXPath(range.endContainer);
    let endOffset= range.endOffset;
    let startXPathSelector=new XPathSelector(startContainerXPath);
    let endXPathSelector=new XPathSelector(endContainerXPath);
    let startOffsetSelector=new RangeOffsetSelector(startOffset);
    let endOffsetSelector=new RangeOffsetSelector(endOffset);
    startXPathSelector.refinedBy=startOffsetSelector;
    endXPathSelector.refinedBy=endOffsetSelector;
    let rangeSelector=new RangeSelector();
    rangeSelector.startSelector=startXPathSelector;
    rangeSelector.endSelector=endXPathSelector;
    return rangeSelector;
}

/**
 * Resolves an XPath expression against the document and returns the resulting node.
 *
 * This version uses the generated namespace‑agnostic XPath (with local-name() tests).
 *
 * @param {string} xpath - The XPath expression.
 * @returns {Node|null} The node found, or null if not found.
 */
function getNodeByXPath(xpath) {
  const evaluator = new XPathEvaluator();
  // No special namespace resolver is needed because we used local-name() in our XPath.
  const result = evaluator.evaluate(
    xpath,
    document,
    null,
    XPathResult.FIRST_ORDERED_NODE_TYPE,
    null
  );
  return result.singleNodeValue;
}

/**
 * Reconstructs a Range object from its JSON-serialized form.
 *
 * @param {string} jsonStr - The JSON string representing the serialized Range.
 * @returns {Range} The deserialized Range object.
 */
function deserializeRange(jsonStr) {
  const data = JSON.parse(jsonStr);
  const startNode = getNodeByXPath(data.startContainerXPath);
  const endNode = getNodeByXPath(data.endContainerXPath);

  if (!startNode || !endNode) {
    throw new Error("Could not locate the start or end node for the Range.");
  }

  const range = document.createRange();
  range.setStart(startNode, data.startOffset);
  range.setEnd(endNode, data.endOffset);
  return range;
}
/**
 * 
 * @param {RangeSelector} rangeSelector 
 * @returns {Range}
 */
function selectorToRange(rangeSelector){
    let startXPathSelector=rangeSelector.startSelector;
    let endXPathSelector=rangeSelector.endSelector;
    //if( !(startXPathSelector instanceof XPathSelector)
    //    || !(endXPathSelector instanceof XPathSelector)
    //)return null;
    let startNode=getNodeByXPath(startXPathSelector.value);
    let endNode=getNodeByXPath(endXPathSelector.value);
    // console.log("get xpath")
    if(startNode===null||endNode===null)return null;
    let range=document.createRange();
    //if( startXPathSelector.refinedBy!==undefined 
    //    && (startXPathSelector.refinedBy instanceof RangeOffsetSelector)
    //)
    {
        let offset=startXPathSelector.refinedBy.value;
        if(typeof offset === "number"){
            range.setStart(startNode,offset);
        }
        // else if(offset==="after"){
        //     range.setStartAfter(startNode);
        // }
        else {
            range.setStartBefore(startNode);
        }
    }
    //else {
    //    range.setStartBefore(startNode);
    //}
    //if( endXPathSelector.refinedBy!==undefined 
    //    && (endXPathSelector.refinedBy instanceof RangeOffsetSelector)
    //)
    {
        let offset=endXPathSelector.refinedBy.value;
        if(typeof offset === "number"){
            range.setEnd(endNode,offset);
        }
        // else if(offset==="before"){
        //     range.setEndBefore(startNode);
        // }
        else {
            range.setEndAfter(endNode);
        }
    }
    //else {
    //    range.setEndAfter(endNode);
    //}
    return range;
}
/**
 * 
 * @returns {RangeSelector}
 */
function getSelectionRangeSelector(){
    let range=window.getSelection().getRangeAt(0);
    return rangeToSelector(range);
}


const CSS_pseudo_element_highlights={};
const CSS_pseudo_element_highlight_colors=["pink" , "orange" , "yellow" , "green" , "blue" , "purple"];
function highlighRegister(){
  // if(Object.keys(CSS_pseudo_element_highlights).length>0)return;
  for(const c of CSS_pseudo_element_highlight_colors){
    const colorHighlight = new Highlight();
    CSS_pseudo_element_highlights[c]=(colorHighlight);
    CSS.highlights.set(`rainbow-color-${c}`, colorHighlight);
  }
}
/**
 * 
 * @param {Range} range 
 * @param {string} color 
 */
function addHighlightRange(range,color="yellow"){
  color=color.toLowerCase();
  if(!(color in CSS_pseudo_element_highlights))
    color="yellow";
  CSS_pseudo_element_highlights[color].add(range);
}
/**
 * 
 * @param {Range} range 
 * @param {string} color 
 */
function delHighlightRange(range,color="yellow"){
  color=color.toLowerCase();
  if(!(color in CSS_pseudo_element_highlights))
    color="yellow";
  CSS_pseudo_element_highlights[color].delete(range);
}

const annotaion_dict=new Map();

const annotation_button_dict=new Map();
/**
 * 
 * @param {Annotation} anno 
 */
function addAnnotation(anno){
  let rangeSelector=anno.target.selector;
  let range=selectorToRange(rangeSelector);
  let arPair=new AnnotationRangePair();
  arPair.annotation=anno;
  arPair.range=range;
  if(anno.body&&anno.body.color)
    addHighlightRange(range,anno.body.color);
  else
    addHighlightRange(range);
  annotaion_dict.set(anno.id,arPair);
}

/**
 * 
 * @param {string} id 
 */
function delAnnotation(id){
  const arPair=annotaion_dict.get(id);
  if(!arPair)return;
  if(arPair.annotation.body&&arPair.annotation.body.color)
    delHighlightRange(arPair.range,arPair.annotation.body.color);
  else
    delHighlightRange(arPair.range);
  const ele = document.getElementById(id);
  if (ele) ele.remove();
  annotation_button_dict.delete(id);
  annotaion_dict.delete(id);
}
function setAnnotationText(id, txt) {
    const arPair = annotaion_dict.get(id);
    if (!arPair) return;
    if (!arPair.annotaiton.body) {
        arPair.annotation.body = new AnnoBody(txt);
    } else {
        arPair.annotation.body.value = txt;
    }

}

let annotation_button_callback = function (id) {
    alert("Annotation button clicked: " + id);
}
function updateAnnotationOverlay(){
  
  annotaion_dict.forEach(
    (arPair,id,m)=>{
      const anno=arPair.annotation;
      const range=arPair.range;
      const rects = range.getClientRects();
      const rect = rects.length ? rects[0] : range.getBoundingClientRect();
      let buttonEl=annotation_button_dict.get(id);
      if (!buttonEl) {
        buttonEl = document.createElement('button');
        buttonEl.id=id;
        buttonEl.className = "annotation_button";
        const imgEl = document.createElement('img');
        imgEl.src = "http://appassets/chat-icon.png";
        imgEl.alt = "Annotation";
        buttonEl.appendChild(imgEl);
        buttonEl.addEventListener('click', function(event) {
            event.stopPropagation();
            annotation_button_callback(event.currentTarget.id);
        });
        document.body.appendChild(buttonEl);
        annotation_button_dict.set(id,buttonEl);
      }
      const startNode=range.startContainer;
      let computedFontSize;
      if (startNode.nodeType === Node.TEXT_NODE) {
          computedFontSize = window.getComputedStyle(startNode.parentElement).fontSize;
      } else {
          computedFontSize = window.getComputedStyle(startNode).fontSize;
      }
      buttonEl.style.fontSize = computedFontSize;
      Object.assign(buttonEl.style, {
          position: 'absolute',
          left: `${rect.left + window.scrollX}px`,
          top: `${rect.top + window.scrollY}px`,
          zIndex: 1000,
          cursor: 'pointer'
        });
    }
  )
}
function updateOverlayLoop() {
    updateAnnotationOverlay();
    requestAnimationFrame(updateOverlayLoop);
}



(function () {
    const link = document.createElement('link');
    link.rel = 'stylesheet';
    link.type = 'text/css';
    link.href = 'http://appassets/annotation-styles.css';
    document.head.appendChild(link);
    highlighRegister();
    updateOverlayLoop();
})()